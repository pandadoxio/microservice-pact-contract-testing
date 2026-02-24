using Microsoft.Extensions.Logging;
using ProductService.Application.Ports;
using ProductService.Domain.ProductAggregate.Abstractions;
using ProductService.Domain.ProductAggregate.Events;

namespace ProductService.Application.DomainEventHandlers;

public class StockReservedHandler(
    IProductRepository repository,
    ILogger<StockReservedHandler> logger)
    : IHandles<StockReservedEvent>
{
    private readonly ILogger<StockReservedHandler> _logger = logger;
    private readonly IProductRepository _repository = repository;

    public async Task HandleAsync(StockReservedEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Stock reserved for product {ProductId}. Quantity: {QuantityReserved}, Remaining: {RemainingStock}",
            domainEvent.ProductId,
            domainEvent.QuantityReserved,
            domainEvent.RemainingStock);

        var product = await _repository.GetByIdAsync(domainEvent.ProductId, ct);

        if (product?.StockQuantity < 10)
        {
            _logger.LogWarning(
                "Product {ProductId} is below reorder threshold with {RemainingStock} units remaining",
                domainEvent.ProductId,
                domainEvent.RemainingStock);

            // Further internal logic here e.g. trigger a reorder workflow
        }
    }
}
