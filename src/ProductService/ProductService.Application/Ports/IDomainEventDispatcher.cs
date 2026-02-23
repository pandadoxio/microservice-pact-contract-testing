namespace ProductService.Application.Ports;

/// <summary>
/// Represents a service that is responsible for dispatching domain events
/// to their respective handlers asynchronously.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<object> domainEvents, CancellationToken ct = default);
}
