using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderService.Application.Ports;
using OrderService.Domain.OrderAggregate.Abstractions;
using OrderService.Infrastructure.Dispatching;
using OrderService.Infrastructure.HttpClients;
using OrderService.Infrastructure.Messaging;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure()
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddOptions<ProductServiceOptions>().BindConfiguration(ProductServiceOptions.SectionName);

            services.AddHttpClient<IProductCatalogueService, ProductCatalogueHttpClient>(client =>
            {
                var options = services.BuildServiceProvider()
                    .GetRequiredService<IOptions<ProductServiceOptions>>()
                    .Value;

                client.BaseAddress = new Uri(options.BaseUrl);
            });

            services.AddOptions<SqsOptions>().BindConfiguration(SqsOptions.SectionName);
            services.AddAWSService<IAmazonSQS>();
            services.AddSingleton<IEventPublisher, SqsEventPublisher>();

            return services;
        }
    }
}
