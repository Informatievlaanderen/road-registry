namespace RoadRegistry.Hosts.Infrastructure.Options;

using Amazon.Lambda;
using System.Collections.Generic;
using System.Linq;

public class LambdaHealthCheckOptionsBuilder : HealthCheckOptionsBuilder<LambdaHealthCheckOptions>
{
    private readonly HashSet<string> _functions = new();
    private readonly AmazonLambdaClient _client;

    public LambdaHealthCheckOptionsBuilder()
    {
        _client = new AmazonLambdaClient();
    }

    public override bool IsValid => _functions.Any() && _client is not null;

    public override LambdaHealthCheckOptions Build() => new()
    {
        Functions = _functions,
        LambdaClient = _client
    };

    public LambdaHealthCheckOptionsBuilder Check(string functionName) 
    {
        _functions.Add(functionName);
        return this;
    }
}
