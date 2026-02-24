using FakeItEasy;
using PactNet;
using PactNet.Matchers;
using ProductService.Application.IntegrationEventHandlers;
using ProductService.Application.IntegrationEvents;
using ProductService.Application.Ports;
using ProductService.Application.UseCases;

namespace ProductService.ContractTests.Messaging;

/// <summary>
///     Consumer-side message Pact tests.
///     ProductService is the MESSAGE CONSUMER — it receives OrderPlaced events.
///     OrderService is the MESSAGE PRODUCER — it publishes them.
///     These tests define what shape ProductService needs the message to have
///     and verify that ProductService's handler can process a conforming message.
///     Generates: /pacts/ProductService-OrderService.json
/// </summary>
[TestFixture]
public class OrderPlacedMessageConsumerTests
{
    [SetUp]
    public void SetUp()
    {
        var pact = Pact.V4("ProductService", "OrderService", PactConfigFactory.ConsumerConfig());
        _pactBuilder = pact.WithMessageInteractions();
    }

    private IMessagePactBuilderV4 _pactBuilder = null!;

    [Test]
    public async Task OrderPlaced_WithSingleLine_HandlerProcessesSuccessfully() =>
        await _pactBuilder
            .ExpectsToReceive("an OrderPlaced event with a single line item")
            .WithJsonContent(new
            {
                orderId = Match.Type(Guid.Parse("aabbccdd-0000-0000-0000-000000000001")),
                customerId = Match.Type(Guid.Parse("aabbccdd-0000-0000-0000-000000000002")),
                placedAt = Match.Type("2024-06-01T10:00:00+00:00"),
                lines = Match.MinType(
                    new
                    {
                        productId = Match.Type(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1")),
                        quantity = Match.Integer(2),
                        unitPrice = Match.Decimal(149.99m)
                    }, 1)
            })
            .VerifyAsync<OrderPlacedIntegrationEvent>(async message =>
            {
                // Arrange
                var fakeReserveStock = A.Fake<IReserveStock>();
                var handler = new OrderPlacedIntegrationHandler(fakeReserveStock);

                // Act
                await handler.HandleAsync(message);

                // Assert
                A.CallTo(() => fakeReserveStock.ExecuteAsync(
                        A<ReserveStockCommand>.That.Matches(cmd =>
                            cmd.ProductId == message.Lines[0].ProductId &&
                            cmd.OrderId == message.OrderId &&
                            cmd.Quantity == message.Lines[0].Quantity)))
                    .MustHaveHappenedOnceExactly();
            });

    [Test]
    public async Task OrderPlaced_WithMultipleLines_HandlerProcessesAllLines() =>
        await _pactBuilder
            .ExpectsToReceive("an OrderPlaced event with multiple line items")
            .WithJsonContent(new
            {
                orderId = Match.Type(Guid.Parse("aabbccdd-0000-0000-0000-000000000003")),
                customerId = Match.Type(Guid.Parse("aabbccdd-0000-0000-0000-000000000004")),
                placedAt = Match.Type("2024-06-01T11:00:00+00:00"),
                lines = Match.MinType(
                    new
                    {
                        productId = Match.Type(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1")),
                        quantity = Match.Integer(1),
                        unitPrice = Match.Decimal(89.99m)
                    }, 2)
            })
            .VerifyAsync<OrderPlacedIntegrationEvent>(async message =>
            {
                // Arrange
                var fakeReserveStock = A.Fake<IReserveStock>();
                var handler = new OrderPlacedIntegrationHandler(fakeReserveStock);

                // Act
                await handler.HandleAsync(message);

                // Assert
                A.CallTo(() => fakeReserveStock.ExecuteAsync(
                        A<ReserveStockCommand>.Ignored))
                    .MustHaveHappened(message.Lines.Count, Times.Exactly);
            });
}
