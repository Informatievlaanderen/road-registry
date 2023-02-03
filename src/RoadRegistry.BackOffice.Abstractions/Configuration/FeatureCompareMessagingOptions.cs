namespace RoadRegistry.BackOffice.Abstractions.Configuration;

public class FeatureCompareMessagingOptions
{
    public const string ConfigurationKey = "FeatureCompareMessagingOptions";

    public int ConsumerDelaySeconds { get; set; } = 30;
    public string RequestQueueUrl { get; set; }
    public string ResponseQueueUrl { get; set; }
}
