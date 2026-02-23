namespace ProductService.Infrastructure.Messaging;

public class SqsOptions
{
    public const string SectionName = "Sqs";

    // <summary>
    // Inbound — ProductService reads from this
    // </summary>
    public string OrderPlacedQueueUrl { get; set; } = string.Empty;

    // <summary>
    // Outbound — ProductService writes to this
    // </summary>
    public string StockReservedQueueUrl { get; set; } = string.Empty;
}
