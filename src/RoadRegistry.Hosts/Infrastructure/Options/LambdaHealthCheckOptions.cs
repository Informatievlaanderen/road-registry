namespace RoadRegistry.Hosts.Infrastructure.Options;

using System.Collections.Generic;
using Amazon.Lambda;

public class LambdaHealthCheckOptions
{
    public IEnumerable<string> Functions { get; set; }
    public AmazonLambdaClient LambdaClient { get; set; }
}
