using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.DomainEventHandlers;
using ProductService.Application.IntegrationEventHandlers;
using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;
using ProductService.Application.UseCases;
using ProductService.Domain.ProductAggregate.Events;

namespace ProductService.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            // Register use cases
            services.AddScoped<GetProductById>();
            services.AddScoped<IReserveStock, ReserveStock>();

            // Register domain event handlers
            services.AddScoped<IHandles<StockReservedEvent>, StockReservedHandler>();

            // Register integration event handlers
            services.AddScoped<IHandlesIntegrationEvent<OrderPlacedIntegrationEvent>, OrderPlacedIntegrationHandler>();

            return services;
        }
    }
}
