using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;

namespace ProductService.Infrastructure.Messaging;

public class SqsOrderPlacedConsumer(
    IAmazonSQS sqsClient,
    IServiceScopeFactory scopeFactory,
    IOptions<SqsOptions> options,
    ILogger<SqsOrderPlacedConsumer> logger)
    : BackgroundService
{
    private readonly ILogger<SqsOrderPlacedConsumer> _logger = logger;
    private readonly SqsOptions _options = options.Value;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IAmazonSQS _sqsClient = sqsClient;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProductService SQS consumer started. Queue: {Queue}", _options.OrderPlacedQueueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _options.OrderPlacedQueueUrl,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 20 // long polling
                }, stoppingToken);

                if (response?.Messages != null)
                {
                    foreach (var sqsMessage in response.Messages)
                    {
                        await ProcessMessageAsync(sqsMessage, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from SQS.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    ///     Handles the full SQS message lifecycle — deserialisation, processing,
    ///     and deletion from the queue on success.
    /// </summary>
    private async Task ProcessMessageAsync(Message sqsMessage, CancellationToken ct = default)
    {
        try
        {
            var orderPlaced = JsonSerializer.Deserialize<OrderPlacedIntegrationEvent>(
                                  sqsMessage.Body,
                                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                              ?? throw new InvalidOperationException(
                                  "Failed to deserialise OrderPlacedIntegrationEvent.");

            await HandleOrderPlacedAsync(orderPlaced, ct);

            await _sqsClient.DeleteMessageAsync(_options.OrderPlacedQueueUrl, sqsMessage.ReceiptHandle, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process SQS message {MessageId}", sqsMessage.MessageId);
            throw;
        }
    }

    /// <summary>
    ///     Core handler logic, separated from SQS concerns so it can be exercised
    ///     in ProductService's own unit tests without needing a real queue.
    /// </summary>
    private async Task HandleOrderPlacedAsync(OrderPlacedIntegrationEvent integrationEvent,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Processing OrderPlaced {OrderId} with {LineCount} line(s)",
            integrationEvent.OrderId, integrationEvent.Lines.Count);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IHandlesIntegrationEvent<OrderPlacedIntegrationEvent>>();

        await handler.HandleAsync(integrationEvent, ct);
    }
}
