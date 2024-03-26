using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy;

using Microsoft.Extensions.Logging;

public class Function : RoadRegistry.Hosts.RoadRegistryLambdaProxyFunction
{
    public Function() : base(
        "http://host.docker.internal:5050",
        "RoadRegistry.BackOffice.Handlers.Sqs.Lambda::RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Function::Handler",
        "aws-lambda-tools-defaults.json") { }
}
