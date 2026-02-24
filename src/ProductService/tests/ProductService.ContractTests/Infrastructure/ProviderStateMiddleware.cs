using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.ProductAggregate;
using ProductService.Domain.ProductAggregate.Abstractions;

namespace ProductService.ContractTests.Infrastructure;

public record ProviderStateRequest(string? State, Dictionary<string, string>? Params);

/// <summary>
///     Test-only middleware registered by WebApplicationFactory.
///     Handles POST /provider-states from the Pact verifier to seed
///     the repository before each interaction is replayed.
/// </summary>
public class ProviderStateMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path != "/provider-states" ||
            context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();

        var providerState = JsonSerializer.Deserialize<ProviderStateRequest>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (providerState?.State is not null)
        {
            var repository = context.RequestServices
                .GetRequiredService<IProductRepository>();

            await SetupStateAsync(providerState.State, providerState.Params, repository);
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
    }

    private static async Task SetupStateAsync(
        string state,
        Dictionary<string, string>? parameters,
        IProductRepository repository)
    {
        switch (state)
        {
            case "a product with id 3fa85f64-5717-4562-b3fc-2c963f66afa1 exists":
            {
                var product = Product.Create(
                    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1"),
                    "Wireless Headphones",
                    "Premium noise-cancelling wireless headphones",
                    149.99m,
                    50);
                await repository.AddAsync(product);
                break;
            }

            case "a product with a known ID exists":
            {
                var id = parameters is not null &&
                         parameters.TryGetValue("id", out var idStr)
                    ? Guid.Parse(idStr)
                    : Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1");

                var product = Product.Create(
                    id,
                    "Wireless Headphones",
                    "Premium noise-cancelling wireless headphones",
                    149.99m,
                    50);
                await repository.AddAsync(product);
                break;
            }

            case "no product exists with the requested ID":
                // Empty repository is the default — nothing to seed
                break;
        }
    }
}
