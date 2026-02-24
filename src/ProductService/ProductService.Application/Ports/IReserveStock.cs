using ProductService.Application.UseCases;

namespace ProductService.Application.Ports;

public interface IReserveStock
{
    Task ExecuteAsync(ReserveStockCommand command, CancellationToken ct = default);
}
