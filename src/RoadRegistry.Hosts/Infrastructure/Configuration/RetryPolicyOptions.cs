namespace RoadRegistry.Hosts.Infrastructure.Configuration;

using BackOffice;

public sealed class RetryPolicyOptions: IHasConfigurationKey
{
    public int MaxRetryCount { get; set; }
    public int StartingRetryDelaySeconds { get; set; }

    public string GetConfigurationKey()
    {
        return "RetryPolicy";
    }
}
