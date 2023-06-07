namespace RoadRegistry.BackOffice.Abstractions.Configuration;

public class FeatureCompareMessagingOptions: IHasConfigurationKey
{
    public int ConsumerDelaySeconds { get; set; } = 30;
    public string RequestQueueUrl { get; set; }
    public string ResponseQueueUrl { get; set; }

    public string GetConfigurationKey()
    {
        return "FeatureCompareMessagingOptions";
    }
}
