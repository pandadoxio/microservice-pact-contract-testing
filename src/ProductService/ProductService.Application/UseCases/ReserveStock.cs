using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;
using ProductService.Domain.ProductAggregate.Abstractions;
using ProductService.Domain.ProductAggregate.Events;

namespace ProductService.Application.UseCases;

public record ReserveStockCommand(Guid ProductId, Guid OrderId, int Quantity);

public class ReserveStock(
    IProductRepository repository,
    IDomainEventDispatcher domainEventDispatcher,
    IEventPublisher eventPublisher)
{
    private readonly IProductRepository _repository = repository;
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public async Task ExecuteAsync(ReserveStockCommand command, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(command.ProductId, ct)
                      ?? throw new InvalidOperationException($"Product {command.ProductId} not found.");

        product.ReserveStock(command.Quantity);
        await _repository.UpdateAsync(product, ct);

        await _domainEventDispatcher.DispatchAsync(product.DomainEvents, ct);
        product.ClearDomainEvents();

        await _eventPublisher.PublishAsync(new StockReservedIntegrationEvent(
            EventId:          Guid.NewGuid(),
            ProductId:        product.Id,
            ProductName:      product.Name,
            OrderId:          command.OrderId,
            QuantityReserved: command.Quantity,
            RemainingStock:   product.StockQuantity,
            OccurredAt:       DateTimeOffset.UtcNow), ct);
    }
}
