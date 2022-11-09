namespace RoadRegistry.BackOffice.Abstractions.Configuration;

public class DynamoDbClientOptions
{
    public const string ConfigurationKey = "DynamoDbClientOptions";
    public string AwsAccessKeyId { get; set; }
    public string AwsSecretAccessKey { get; set; }
    public string Region { get; set; }
    public string ServerUrl { get; set; }
}
