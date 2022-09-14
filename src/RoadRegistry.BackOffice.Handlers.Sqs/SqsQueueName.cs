namespace RoadRegistry.BackOffice.Handlers.Sqs;

internal static class SqsQueueName
{
    public const string Value = "road-registry-feature-compare";
}

internal static class SqsFeatureCompare
{
    public static readonly string MessageGroupId = Guid.Parse("1ea553f1-3af6-4594-85e8-ac9f463525d7").ToString();
}
