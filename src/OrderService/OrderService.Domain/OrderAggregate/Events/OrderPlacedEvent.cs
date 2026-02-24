namespace OrderService.Domain.OrderAggregate.Events;

/// <summary>
///     Domain event raised when an order is successfully placed.
/// </summary>
public record OrderPlacedEvent(
    Guid OrderId,
    Guid CustomerId,
    DateTimeOffset PlacedAt,
    IReadOnlyList<OrderLine> Lines);
