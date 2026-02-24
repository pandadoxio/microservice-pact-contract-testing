namespace OrderService.Application.Ports;

public interface IHandles<in T> where T : class
{
    Task HandleAsync(T domainEvent, CancellationToken ct = default);
}
