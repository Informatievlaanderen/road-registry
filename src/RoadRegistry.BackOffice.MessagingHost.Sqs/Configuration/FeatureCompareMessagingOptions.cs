namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Configuration;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

public class FeatureCompareMessagingOptions
{
    public int ConsumerDelaySeconds { get; set; } = 30;

    public string RequestQueueName => SqsQueue.ParseQueueNameFromQueueUrl(RequestQueueUrl);
    public string RequestQueueUrl { get; set; }
    public string ResponseQueueName => SqsQueue.ParseQueueNameFromQueueUrl(ResponseQueueUrl);
    public string ResponseQueueUrl { get; set; }
}
