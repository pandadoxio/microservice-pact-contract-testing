using ProductService.Application.Ports;

namespace ProductService.Infrastructure.Dispatching;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task DispatchAsync(IEnumerable<object> domainEvents, CancellationToken ct = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IHandles<>).MakeGenericType(domainEvent.GetType());
            var handler = _serviceProvider.GetService(handlerType);

            if (handler is not null)
            {
                await ((dynamic)handler).HandleAsync((dynamic)domainEvent, ct);
            }
        }
    }
}
