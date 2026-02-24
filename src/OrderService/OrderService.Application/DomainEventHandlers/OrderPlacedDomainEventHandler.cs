using Microsoft.Extensions.Logging;
using OrderService.Application.Ports;
using OrderService.Domain.OrderAggregate.Events;

namespace OrderService.Application.DomainEventHandlers;

public class OrderPlacedDomainEventHandler : IHandles<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedDomainEventHandler> _logger;

    public OrderPlacedDomainEventHandler(ILogger<OrderPlacedDomainEventHandler> logger)
        => _logger = logger;

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken ct = default)
    {
        // Internal concerns only — fraud checks, audit logging etc.
        _logger.LogInformation(
            "Order {OrderId} placed by customer {CustomerId} with {LineCount} line(s)",
            domainEvent.OrderId,
            domainEvent.CustomerId,
            domainEvent.Lines.Count);

        // Future internal concerns could go here e.g.
        // - fraud detection
        // - loyalty points calculation
        // - internal audit log

        return Task.CompletedTask;
    }
}
