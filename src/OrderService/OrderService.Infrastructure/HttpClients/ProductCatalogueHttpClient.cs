using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OrderService.Domain.OrderAggregate.Abstractions;

namespace OrderService.Infrastructure.HttpClients;

/// <summary>
///     Infrastructure adapter — implements the domain port IProductCatalogueService
///     using HTTP calls to ProductService's REST API.
/// </summary>
public class ProductCatalogueHttpClient(HttpClient httpClient) : IProductCatalogueService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient = httpClient;

    public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/v1/products/{productId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<ProductResponseDto>(JsonOptions, ct)
                  ?? throw new InvalidOperationException("Received null product response from ProductService.");

        return new ProductInfo(dto.Id, dto.Name, dto.Price, dto.InStock);
    }

    // Local DTO — only used in this adapter, never leaked into the domain
    private record ProductResponseDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        bool InStock,
        int StockQuantity);
}
