using OrderService.Domain.OrderAggregate.Events;

namespace OrderService.Domain.OrderAggregate;

public class Order
{
    private readonly List<object> _domainEvents = [];

    private readonly List<OrderLine> _lines = [];

    private Order() { }
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    public static Order Place(Guid customerId, IEnumerable<OrderLine> lines)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0)
        {
            throw new InvalidOperationException("An order must have at least one line.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Placed,
            PlacedAt = DateTimeOffset.UtcNow
        };

        order._lines.AddRange(lineList);

        order._domainEvents.Add(new OrderPlacedEvent(
            order.Id,
            customerId,
            order.PlacedAt,
            lineList));

        return order;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

public enum OrderStatus { Placed, Confirmed, Shipped, Delivered, Cancelled }
