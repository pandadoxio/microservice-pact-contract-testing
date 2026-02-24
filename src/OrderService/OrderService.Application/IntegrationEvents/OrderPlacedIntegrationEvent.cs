namespace OrderService.Application.IntegrationEvents;

public record OrderPlacedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CustomerId,
    DateTimeOffset PlacedAt,
    IReadOnlyList<OrderLineDto> Lines);

public record OrderLineDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);
