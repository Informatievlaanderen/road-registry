namespace RoadRegistry.BackOffice.Abstractions.Configuration;

public class DynamoDbClientOptions
{
    public const string ConfigurationKey = "DynamoDbClientOptions";
    public string AccessKeyId { get; set; }
    public string AccessKeySecret { get; set; }
    public string ServerUrl { get; set; }
}
