namespace OrderService.Domain.OrderAggregate.Abstractions;

/// <summary>
///     Outbound port — lets the domain validate products without knowing
///     anything about HTTP or ProductService's internals.
/// </summary>
public interface IProductCatalogueService
{
    Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken ct = default);
}

public record ProductInfo(Guid Id, string Name, decimal Price, bool InStock);
