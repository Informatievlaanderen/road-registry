namespace RoadRegistry.Hosts.Infrastructure.Options;

using Amazon;
using Amazon.Runtime;

public class SqsHealthCheckOptions
{
    public string ServiceUrl { get; set; }
    public string QueueUrl { get; set; }
    public RegionEndpoint RegionEndpoint { get; set; }
    public AWSCredentials Credentials { get; set; }
}
