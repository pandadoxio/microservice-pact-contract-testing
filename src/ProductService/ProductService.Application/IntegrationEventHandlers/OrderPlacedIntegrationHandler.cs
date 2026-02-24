using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;
using ProductService.Application.UseCases;

namespace ProductService.Application.IntegrationEventHandlers;

public class OrderPlacedIntegrationHandler(IReserveStock reserveStock) : IHandlesIntegrationEvent<OrderPlacedIntegrationEvent>
{
    private readonly IReserveStock _reserveStock = reserveStock;

    public async Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken ct = default)
    {
        foreach (var line in integrationEvent.Lines)
        {
            var command = new ReserveStockCommand(
                line.ProductId,
                integrationEvent.OrderId,
                line.Quantity);

            await _reserveStock.ExecuteAsync(command, ct);
        }
    }
}
