using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Domain.ProductAggregate.Abstractions;
using ProductService.Infrastructure.Repositories;

namespace ProductService.ContractTests.Infrastructure;

/// <summary>
///     Boots ProductService on a real Kestrel port so the Pact verifier
///     can make real HTTP calls to it. WebApplicationFactory's in-memory
///     TestServer cannot be used with the Pact verifier directly.
/// </summary>
public class ProductServiceWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestServerUrl = "http://localhost:9222";

    public Uri ServerUri => new(TestServerUrl);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Bind to a real Kestrel port so Pact verifier can reach it over HTTP
        builder.UseUrls(TestServerUrl);

        builder.ConfigureServices(services =>
        {
            // Replace the repository with a clean empty singleton instance.
            // Singleton ensures the state seeded by ProviderStateMiddleware
            // is visible to the endpoint handlers within the same request cycle.
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IProductRepository));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        });

        builder.Configure(app =>
        {
            app.UseMiddleware<ProviderStateMiddleware>();
        });
    }
}
