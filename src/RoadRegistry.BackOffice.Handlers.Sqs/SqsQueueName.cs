namespace RoadRegistry.BackOffice.Handlers.Sqs;

internal static class SqsQueueName
{
    public static class FeatureCompare
    {
        public const string RequestQueue = "sqs-road-registry-feature-compare-request.fifo";
        public const string ResponseQueue = "sqs-road-registry-feature-compare-response.fifo";
    }
}

internal static class SqsFeatureCompare
{
    public static readonly string MessageGroupId = Guid.Parse("1ea553f1-3af6-4594-85e8-ac9f463525d7").ToString();
}
