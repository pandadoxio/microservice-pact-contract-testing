namespace ProductService.Application.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    bool InStock,
    int StockQuantity);
