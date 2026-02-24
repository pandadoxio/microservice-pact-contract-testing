namespace OrderService.Domain.OrderAggregate;

public record OrderLine(Guid ProductId, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;

    public static OrderLine Create(Guid productId, int quantity, decimal unitPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        return new OrderLine(productId, quantity, unitPrice);
    }
}
