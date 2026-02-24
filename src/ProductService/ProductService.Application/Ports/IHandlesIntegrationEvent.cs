using ProductService.Application.IntegrationEvents;

namespace ProductService.Application.Ports;

public interface IHandlesIntegrationEvent<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent integrationEvent, CancellationToken ct = default);
}
