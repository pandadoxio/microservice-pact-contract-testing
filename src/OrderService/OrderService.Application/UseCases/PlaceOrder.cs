using OrderService.Application.Dtos;
using OrderService.Application.IntegrationEvents;
using OrderService.Application.Ports;
using OrderService.Domain.OrderAggregate;
using OrderService.Domain.OrderAggregate.Abstractions;

namespace OrderService.Application.UseCases;

public record PlaceOrderCommand(Guid CustomerId, List<OrderLineCommand> Lines);

public record OrderLineCommand(Guid ProductId, int Quantity);

public class PlaceOrder(
    IProductCatalogueService productCatalogue,
    IDomainEventDispatcher domainEventDispatcher,
    IEventPublisher eventPublisher)
{
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly IProductCatalogueService _productCatalogue = productCatalogue;

    public async Task<OrderDto> ExecuteAsync(PlaceOrderCommand command, CancellationToken ct = default)
    {
        var lines = new List<OrderLine>();

        foreach (var line in command.Lines)
        {
            var product = await _productCatalogue.GetProductAsync(line.ProductId, ct)
                          ?? throw new InvalidOperationException($"Product {line.ProductId} not found.");

            if (!product.InStock)
            {
                throw new InvalidOperationException($"Product '{product.Name}' is out of stock.");
            }

            lines.Add(OrderLine.Create(line.ProductId, line.Quantity, product.Price));
        }

        var order = Order.Place(command.CustomerId, lines);

        await _domainEventDispatcher.DispatchAsync(order.DomainEvents, ct);
        order.ClearDomainEvents();

        await _eventPublisher.PublishAsync(new OrderPlacedIntegrationEvent(
            Guid.NewGuid(),
            order.Id,
            order.CustomerId,
            order.PlacedAt,
            order.Lines.Select(line => new OrderLineDto(
                line.ProductId,
                line.Quantity,
                line.UnitPrice)).ToList()), ct);

        return new OrderDto(order.Id, order.Status.ToString(), order.PlacedAt);
    }
}
