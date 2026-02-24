namespace ProductService.Application.Ports;

public interface IHandles<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
