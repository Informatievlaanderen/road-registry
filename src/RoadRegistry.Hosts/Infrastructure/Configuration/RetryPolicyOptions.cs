namespace RoadRegistry.Hosts.Infrastructure.Configuration;

public sealed class RetryPolicyOptions
{
    public const string ConfigurationKey = "RetryPolicy";
    public int MaxRetryCount { get; set; }
    public int StartingRetryDelaySeconds { get; set; }
}
