using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.DomainEventHandlers;
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
            services.AddScoped<GetProductById>();
            services.AddScoped<ReserveStock>();

            // Register domain event handlers
            services.AddScoped<IHandles<StockReservedEvent>, StockReservedDomainEventHandler>();

            return services;
        }
    }
}
