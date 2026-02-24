using System.Net;
using OrderService.Infrastructure.HttpClients;
using PactNet;
using PactNet.Matchers;

namespace OrderService.ContractTests.Http;

/// <summary>
///     Consumer-side HTTP Pact tests.
///     OrderService is the HTTP CONSUMER — it calls ProductService's REST API.
///     ProductService is the HTTP PROVIDER — it serves the API.
///     These tests define what shape OrderService needs from the API and exercise
///     the real ProductCatalogueHttpClient adapter against Pact's mock server.
///     Generates: /pacts/OrderService-ProductService.json
/// </summary>
[TestFixture]
public class ProductCatalogueHttpConsumerTests
{
    [SetUp]
    public void SetUp()
    {
        var pact = Pact.V4("OrderService", "ProductService", PactConfigFactory.ConsumerConfig());
        _pactBuilder = pact.WithHttpInteractions();
    }

    private IPactBuilderV4 _pactBuilder = null!;

    private static readonly Guid KnownProductId =
        Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa1");

    private static readonly Guid UnknownProductId =
        Guid.Parse("00000000-0000-0000-0000-000000000000");

    private static readonly Guid OutOfStockProductId =
        Guid.Parse("5fc07a86-7939-6784-c5ad-4e185a88bac3");

    [Test]
    public async Task GetProduct_WhenProductExists_ReturnsProductDetails()
    {
        _pactBuilder
            .UponReceiving("a request for a product that exists")
            .Given("a product with ID {id} exists",
                new Dictionary<string, string> { ["id"] = KnownProductId.ToString() })
            .WithRequest(HttpMethod.Get, $"/api/v1/products/{KnownProductId}")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", Match.Regex(
                "application/json",
                "application/json.*"))
            .WithJsonBody(new
            {
                id = Match.Type(KnownProductId),
                name = Match.Type("Wireless Headphones"),
                description = Match.Type("Premium noise-cancelling wireless headphones"),
                price = Match.Decimal(149.99m),
                inStock = Match.Type(true),
                stockQuantity = Match.Integer(50)
            });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = ctx.MockServerUri };
            var sut = new ProductCatalogueHttpClient(httpClient);

            // Act
            var result = await sut.GetProductAsync(KnownProductId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(KnownProductId));
            Assert.That(result.Name, Is.Not.Empty);
            Assert.That(result.Price, Is.GreaterThan(0));
            Assert.That(result.InStock, Is.True);
        });
    }

    [Test]
    public async Task GetProduct_WhenProductDoesNotExist_ReturnsNull()
    {
        _pactBuilder
            .UponReceiving("a request for a product that does not exist")
            .Given("no product exists with ID {id}",
                new Dictionary<string, string> { ["id"] = UnknownProductId.ToString() })
            .WithRequest(HttpMethod.Get, $"/api/v1/products/{UnknownProductId}")
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = ctx.MockServerUri };
            var sut = new ProductCatalogueHttpClient(httpClient);

            // Act
            var result = await sut.GetProductAsync(UnknownProductId);

            // Assert
            Assert.That(result, Is.Null);
        });
    }

    [Test]
    public async Task GetProduct_WhenProductIsOutOfStock_ReturnsProductWithInStockFalse()
    {
        _pactBuilder
            .UponReceiving("a request for a product that is out of stock")
            .Given("a product with ID {id} is out of stock",
                new Dictionary<string, string> { ["id"] = OutOfStockProductId.ToString() })
            .WithRequest(HttpMethod.Get, $"/api/v1/products/{OutOfStockProductId}")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", Match.Regex(
                "application/json",
                "application/json.*"))
            .WithJsonBody(new
            {
                id = Match.Type(OutOfStockProductId),
                name = Match.Type("USB-C Hub"),
                description = Match.Type("7-in-1 USB-C hub with 4K HDMI"),
                price = Match.Decimal(49.99m),
                inStock = false, // exact value — consumer specifically needs to know it's false
                stockQuantity = Match.Integer(0)
            });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Arrange
            var httpClient = new HttpClient { BaseAddress = ctx.MockServerUri };
            var sut = new ProductCatalogueHttpClient(httpClient);

            // Act
            var result = await sut.GetProductAsync(OutOfStockProductId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.InStock, Is.False);
        });
    }
}
