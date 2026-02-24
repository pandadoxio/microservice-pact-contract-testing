namespace OrderService.Infrastructure.HttpClients;

public class ProductServiceOptions
{
    public const string SectionName = "ProductService";
    public string BaseUrl { get; set; } = string.Empty;
}
