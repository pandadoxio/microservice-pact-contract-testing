using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;
using ProductService.Domain.ProductAggregate.Abstractions;

namespace ProductService.Application.UseCases;

public record ReserveStockCommand(Guid ProductId, Guid OrderId, int Quantity);

public class ReserveStock(
    IProductRepository repository,
    IDomainEventDispatcher domainEventDispatcher,
    IEventPublisher eventPublisher)
{
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly IProductRepository _repository = repository;

    public async Task ExecuteAsync(ReserveStockCommand command, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(command.ProductId, ct)
                      ?? throw new InvalidOperationException($"Product {command.ProductId} not found.");

        product.ReserveStock(command.Quantity);
        await _repository.UpdateAsync(product, ct);

        await _domainEventDispatcher.DispatchAsync(product.DomainEvents, ct);
        product.ClearDomainEvents();

        await _eventPublisher.PublishAsync(new StockReservedIntegrationEvent(
            Guid.NewGuid(),
            product.Id,
            product.Name,
            command.OrderId,
            command.Quantity,
            product.StockQuantity,
            DateTimeOffset.UtcNow), ct);
    }
}
