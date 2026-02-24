namespace OrderService.Infrastructure.Messaging;

public class SqsOptions
{
    public const string SectionName = "Sqs";
    public string OrderPlacedQueueUrl { get; set; } = string.Empty;
}
