namespace ProductService.Infrastructure.Messaging;

/// <summary>
///     The message structure ProductService expects to receive from OrderService
///     via the SQS queue.
/// </summary>
public class OrderPlacedMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public List<OrderLineMessage> Lines { get; set; } = [];
}

public class OrderLineMessage
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
