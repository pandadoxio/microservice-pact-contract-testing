using ProductService.Domain.ProductAggregate;
using ProductService.Domain.ProductAggregate.Abstractions;

namespace ProductService.Infrastructure.Repositories;

/// <summary>
///     Simple in-memory repository — sufficient for a demo.
///     Replace it with an EF Core implementation for production.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _store = new();

    public InMemoryProductRepository() => Seed();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue(id, out var product) ? product : null);

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Product>>(_store.Values.ToList());

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _store[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _store[product.Id] = product;
        return Task.CompletedTask;
    }

    private void Seed()
    {
        var products = new[]
        {
            Product.Create(
                Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1"),
                "Wireless Headphones",
                "Premium noise-cancelling wireless headphones",
                149.99m,
                50),
            Product.Create(
                Guid.Parse("4fb96f75-6828-5673-b4fc-3d074f77afa2"),
                "Mechanical Keyboard",
                "Compact TKL mechanical keyboard with RGB",
                89.99m,
                30),
            Product.Create(
                Guid.Parse("5fc07a86-7939-6784-c5ad-4e185a88bac3"),
                "USB-C Hub",
                "7-in-1 USB-C hub with 4K HDMI",
                49.99m,
                0) // out of stock deliberately
        };

        foreach (var product in products)
        {
            _store[product.Id] = product;
        }
    }
}
