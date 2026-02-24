using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ProductService.Domain.ProductAggregate;
using ProductService.Domain.ProductAggregate.Abstractions;

namespace ProductService.ContractTests.Infrastructure;

/// <summary>
///     Test-only middleware registered by WebApplicationFactory.
///     Handles POST /provider-states from the Pact verifier to seed
///     the repository before each interaction is replayed.
/// </summary>
public class ProviderStateMiddleware
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private readonly RequestDelegate _next;
    private readonly IProductRepository _products;

    private readonly IDictionary<string, Func<IDictionary<string, object>, Task>> _providerStates;

    /// <summary>
    ///     Initialises a new instance of the <see cref="ProviderStateMiddleware" /> class.
    /// </summary>
    /// <param name="next">Next request delegate</param>
    /// <param name="products">Products repository for actioning provider state requests</param>
    public ProviderStateMiddleware(RequestDelegate next, IProductRepository products)
    {
        _next = next;
        _products = products;

        _providerStates = new Dictionary<string, Func<IDictionary<string, object>, Task>>
        {
            ["a product with ID {id} exists"] = EnsureProductExistsAsync,
            ["no product exists with ID {id}"] = EnsureProductDoesNotExistAsync,
            ["a product with ID {id} is out of stock"] = EnsureProductIsOutOfStockAsync
        };
    }

    /// <summary>
    ///     Ensure a product exists
    /// </summary>
    /// <param name="parameters">Event parameters</param>
    /// <returns>Awaitable</returns>
    private async Task EnsureProductExistsAsync(IDictionary<string, object> parameters)
    {
        var id = (JsonElement)parameters["id"];

        await _products.AddAsync(Product.Create(
            id.GetGuid(),
            "Mechanical Numpad",
            "Compact standalone numpad with tactile switches",
            34.99m,
            15));
    }

    /// <summary>
    ///     Ensure a product does not exist
    /// </summary>
    /// <param name="parameters">Event parameters</param>
    /// <returns>Awaitable</returns>
    private async Task EnsureProductDoesNotExistAsync(IDictionary<string, object> parameters)
    {
        var id = (JsonElement)parameters["id"];
        await _products.RemoveAsync(id.GetGuid());
    }

    private async Task EnsureProductIsOutOfStockAsync(IDictionary<string, object> parameters)
    {
        var id = (JsonElement)parameters["id"];

        await _products.AddAsync(Product.Create(
            id.GetGuid(),
            "Ergonomic Wrist Rest",
            "Memory foam wrist rest for keyboard and mouse",
            19.99m,
            0));
    }

    /// <summary>
    ///     Handle the request
    /// </summary>
    /// <param name="context">Request context</param>
    /// <returns>Awaitable</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!(context.Request.Path.Value?.StartsWith("/provider-states") ?? false))
        {
            await _next.Invoke(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;

        if (context.Request.Method == HttpMethod.Post.ToString())
        {
            string jsonRequestBody;

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                jsonRequestBody = await reader.ReadToEndAsync();
            }

            try
            {
                var providerState = JsonSerializer.Deserialize<ProviderState>(jsonRequestBody, Options);

                if (!string.IsNullOrEmpty(providerState?.State))
                {
                    await _providerStates[providerState.State].Invoke(providerState.Params);
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Failed to deserialise JSON provider state body:");
                await context.Response.WriteAsync(jsonRequestBody);
                await context.Response.WriteAsync(string.Empty);
                await context.Response.WriteAsync(e.ToString());
            }
        }
    }
}
