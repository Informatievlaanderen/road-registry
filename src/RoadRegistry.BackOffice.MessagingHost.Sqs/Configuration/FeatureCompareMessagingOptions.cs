using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Configuration
{
    public class FeatureCompareMessagingOptions
    {
        public string RequestQueueUrl { get; set; }
        public string ResponseQueueUrl { get; set; }
        public int ConsumerDelaySeconds { get; set; } = 30;

        public string RequestQueueName => SqsQueue.ParseQueueNameFromQueueUrl(RequestQueueUrl);
        public string ResponseQueueName => SqsQueue.ParseQueueNameFromQueueUrl(ResponseQueueUrl);
    }
}
