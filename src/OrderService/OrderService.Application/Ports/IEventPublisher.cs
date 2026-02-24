namespace OrderService.Application.Ports;

/// <summary>
///     Outbound port — the application layer uses this to publish events
///     without depending on any specific messaging technology.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class;
}
