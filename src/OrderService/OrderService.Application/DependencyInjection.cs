using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.DomainEventHandlers;
using OrderService.Application.Ports;
using OrderService.Application.UseCases;
using OrderService.Domain.OrderAggregate.Events;

namespace OrderService.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<PlaceOrder>();

            // Register domain event handlers
            services.AddScoped<IHandles<OrderPlacedEvent>, OrderPlacedDomainEventHandler>();

            return services;
        }
    }
}
