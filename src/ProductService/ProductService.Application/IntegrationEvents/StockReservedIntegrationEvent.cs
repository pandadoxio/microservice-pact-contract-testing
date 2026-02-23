namespace ProductService.Application.IntegrationEvents;

public record StockReservedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    string ProductName,
    Guid OrderId,
    int QuantityReserved,
    int RemainingStock,
    DateTimeOffset OccurredAt);
