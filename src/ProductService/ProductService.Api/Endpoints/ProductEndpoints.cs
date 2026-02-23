using ProductService.Application.Dtos;
using ProductService.Application.UseCases;

namespace ProductService.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products")
            .WithTags("Products");

        group.MapGet("{id:guid}", GetByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Get a product by its ID")
            .Produces<ProductDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetProductById getProductById,
        CancellationToken ct)
    {
        var product = await getProductById.ExecuteAsync(id, ct);

        return product is null
            ? Results.NotFound()
            : Results.Ok(product);
    }
}
