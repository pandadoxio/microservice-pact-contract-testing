using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Application.Ports;
using ProductService.Domain.ProductAggregate.Abstractions;
using ProductService.Infrastructure.Dispatching;
using ProductService.Infrastructure.Messaging;
using ProductService.Infrastructure.Repositories;

namespace ProductService.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure()
        {
            services.AddSingleton<IProductRepository, InMemoryProductRepository>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            services.AddOptions<SqsOptions>().BindConfiguration(SqsOptions.SectionName);
            services.AddAWSService<IAmazonSQS>();
            services.AddSingleton<IEventPublisher, SqsEventPublisher>();
            services.AddHostedService<SqsOrderPlacedConsumer>();

            return services;
        }
    }
}
