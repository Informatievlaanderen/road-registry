[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Proxy;

public class Function : RoadRegistry.Hosts.RoadRegistryLambdaProxyFunction
{
    public Function() : base(
        "http://localhost:5051",
        "RoadRegistry.Snapshot.Handlers.Sqs.Lambda::RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Function::Handler",
        "aws-lambda-tools-defaults.json") { }
}
