namespace ProductService.Domain.ProductAggregate.Events;

/// <summary>
/// Raised by the domain when stock has been successfully reserved
/// in response to an OrderPlaced event from OrderService.
/// </summary>
public record StockReservedEvent(
    Guid ProductId,
    int QuantityReserved,
    int RemainingStock,
    DateTimeOffset OccurredAt);
