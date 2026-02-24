namespace ProductService.Application.IntegrationEvents;

/// <summary>
///     The message structure ProductService expects to receive from OrderService
/// </summary>
public class OrderPlacedIntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}

public class OrderLine
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
