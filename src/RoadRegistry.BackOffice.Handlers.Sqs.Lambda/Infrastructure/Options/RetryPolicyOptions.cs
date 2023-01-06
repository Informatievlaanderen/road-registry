namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Options;

public sealed class RetryPolicyOptions
{
    public const string ConfigurationKey = "RetryPolicy";

    public int MaxRetryCount { get; set; }
    public int StartingRetryDelaySeconds { get; set; }
}
