using OrderService.Application.Dtos;
using OrderService.Application.UseCases;

namespace OrderService.Api.Endpoints;

public record PlaceOrderRequest(Guid CustomerId, List<OrderItemDto> Items);

public record OrderItemDto(Guid ProductId, int Quantity);

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders");

        group.MapPost("", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .WithSummary("Place a new order")
            .WithDescription("Validates all products are in stock and places the order.")
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> PlaceOrderAsync(
        PlaceOrderRequest request,
        PlaceOrder placeOrder,
        CancellationToken ct)
    {
        try
        {
            var command = new PlaceOrderCommand(
                request.CustomerId,
                request.Items
                    .Select(i => new OrderLineCommand(i.ProductId, i.Quantity))
                    .ToList());

            var result = await placeOrder.ExecuteAsync(command, ct);

            return Results.CreatedAtRoute(
                "PlaceOrder",
                new { id = result.OrderId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
