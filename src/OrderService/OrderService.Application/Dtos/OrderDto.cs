namespace OrderService.Application.Dtos;

public record OrderDto(Guid OrderId, string Status, DateTimeOffset PlacedAt);
