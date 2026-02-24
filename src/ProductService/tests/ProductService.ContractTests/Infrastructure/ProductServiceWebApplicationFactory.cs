using Amazon.SQS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Ports;
using ProductService.Domain.ProductAggregate.Abstractions;
using ProductService.Infrastructure.Messaging;
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
    public Uri ProviderStatesUri => new(ServerUri, "/provider-states");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseKestrel tells WebApplicationFactory to use a real Kestrel server
        // instead of the in-memory TestServer
        UseKestrel(options => options.ListenLocalhost(9222));

        builder.ConfigureServices(services =>
        {
            var hostedServiceDescriptor =
                services.SingleOrDefault(d => d.ImplementationType == typeof(SqsOrderPlacedConsumer));
            if (hostedServiceDescriptor is not null)
            {
                services.Remove(hostedServiceDescriptor);
            }

            var publisherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEventPublisher));
            if (publisherDescriptor is not null)
            {
                services.Remove(publisherDescriptor);
            }

            var sqsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAmazonSQS));
            if (sqsDescriptor is not null)
            {
                services.Remove(sqsDescriptor);
            }

            var repositoryDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IProductRepository));
            if (repositoryDescriptor is not null)
            {
                services.Remove(repositoryDescriptor);
            }

            services.AddSingleton<IProductRepository>(new InMemoryProductRepository(false));
            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
            services.AddSingleton<IStartupFilter, ProviderStateStartupFilter>();
        });
    }
}
