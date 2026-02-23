using ProductService.Domain.ProductAggregate.Events;

namespace ProductService.Domain.ProductAggregate;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool InStock => StockQuantity > 0;

    private readonly List<object> _domainEvents = [];
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private Product() { }   // EF

    public static Product Create(Guid id, string name, string description, decimal price, int stockQuantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stockQuantity);

        return new Product
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity
        };
    }

    /// <summary>Reserve stock when an order is placed</summary>
    /// <param name="quantity">The number of items to reserve.</param>
    public void ReserveStock(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");
        }

        StockQuantity -= quantity;

        _domainEvents.Add(new StockReservedEvent(
            ProductId:        Id,
            QuantityReserved: quantity,
            RemainingStock:   StockQuantity,
            OccurredAt:       DateTimeOffset.UtcNow));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
