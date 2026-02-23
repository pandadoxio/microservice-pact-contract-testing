using ProductService.Application.Dtos;
using ProductService.Domain.ProductAggregate.Abstractions;

namespace ProductService.Application.UseCases;

public class GetProductById(IProductRepository repository)
{
    private readonly IProductRepository _repository = repository;

    public async Task<ProductDto?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct);

        if (product is null)
        {
            return null;
        }

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.InStock,
            product.StockQuantity);
    }
}
