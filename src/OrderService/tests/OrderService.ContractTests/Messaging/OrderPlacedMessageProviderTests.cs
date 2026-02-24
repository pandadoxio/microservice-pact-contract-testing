using System.Text.Json;
using OrderService.Application.IntegrationEvents;
using PactNet.Verifier;

namespace OrderService.ContractTests.Messaging;

/// <summary>
///     Provider-side message Pact verification.
///     OrderService is the MESSAGE PRODUCER — it publishes OrderPlaced events.
///     ProductService is the MESSAGE CONSUMER — it receives them.
///     This test verifies that the message OrderService actually produces matches
///     the shape ProductService declared it needs in its consumer test.
///     No SQS involved — Pact calls the factory methods below, serialises the
///     result, and compares it against ProductService-OrderService.json.
///     Run AFTER ProductService.ContractTests to ensure the pact file exists.
/// </summary>
[TestFixture]
public class OrderPlacedMessageProviderTests
{
    [Test]
    public void OrderPlaced_AsProvider_SatisfiesProductServiceContract()
    {
        var pactFile = Path.Combine(PactConfigFactory.PactDir, "ProductService-OrderService.json");

        Assert.That(File.Exists(pactFile), Is.True,
            $"Pact file not found at {pactFile}. " +
            "Run ProductService.ContractTests first to generate the pact file.");

        using var verifier = new PactVerifier("OrderService", PactConfigFactory.VerifierConfig());

        var defaultSettings = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        verifier
            .WithHttpEndpoint(new Uri("http://localhost:49152"))
            .WithMessages(scenarios =>
            {
                scenarios.Add("an OrderPlaced event with a single line item",
                    BuildOrderPlacedWithSingleLine);

                scenarios.Add("an OrderPlaced event with multiple line items",
                    BuildOrderPlacedWithMultipleLines);
            }, defaultSettings)
            .WithFileSource(new FileInfo(pactFile))
            .Verify();
    }

    private static dynamic BuildOrderPlacedWithSingleLine() =>
        new OrderPlacedIntegrationEvent(
            Guid.NewGuid(),
            Guid.Parse("aabbccdd-0000-0000-0000-000000000001"),
            Guid.Parse("aabbccdd-0000-0000-0000-000000000002"),
            DateTimeOffset.Parse("2024-06-01T10:00:00+00:00"),
            new List<OrderLineDto>
            {
                new(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1"),
                    2,
                    149.99m)
            });

    private static dynamic BuildOrderPlacedWithMultipleLines() =>
        new OrderPlacedIntegrationEvent(
            Guid.NewGuid(),
            Guid.Parse("aabbccdd-0000-0000-0000-000000000003"),
            Guid.Parse("aabbccdd-0000-0000-0000-000000000004"),
            DateTimeOffset.Parse("2024-06-01T11:00:00+00:00"),
            new List<OrderLineDto>
            {
                new(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1"),
                    1,
                    149.99m),
                new(Guid.Parse("4fb96f75-6828-5673-b4fc-3d074f77afa2"),
                    3,
                    89.99m)
            });
}
