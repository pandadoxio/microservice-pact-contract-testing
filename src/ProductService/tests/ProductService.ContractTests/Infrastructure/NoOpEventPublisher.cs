using ProductService.Application.Ports;

namespace ProductService.ContractTests.Infrastructure;

/// <summary>
///     No-op event publisher for use in provider tests — prevents DI errors
///     when IEventPublisher is resolved, but SQS is not available.
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class
        => Task.CompletedTask;
}
