using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;

namespace ProductService.Infrastructure.Messaging;

public class SqsEventPublisher(IAmazonSQS sqsClient, IOptions<SqsOptions> options) : IEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly SqsOptions _options = options.Value;
    private readonly IAmazonSQS _sqsClient = sqsClient;

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
    {
        var queueUrl = @event switch
        {
            StockReservedIntegrationEvent => _options.StockReservedQueueUrl,
            _ => throw new InvalidOperationException($"No queue configured for event type {typeof(T).Name}")
        };

        var body = JsonSerializer.Serialize(@event, SerializerOptions);

        await _sqsClient.SendMessageAsync(
            new SendMessageRequest
            {
                QueueUrl = _options.OrderPlacedQueueUrl,
                MessageBody = body,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["EventType"] = new() { DataType = "String", StringValue = typeof(T).Name }
                }
            }, ct);
    }
}
