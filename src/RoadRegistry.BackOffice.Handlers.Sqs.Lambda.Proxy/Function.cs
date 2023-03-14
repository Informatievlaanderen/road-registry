[assembly: LambdaSerializer(typeof(JsonSerializer))]
namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Proxy;

public class Function : RoadRegistry.Hosts.RoadRegistryLambdaProxyFunction
{
    public Function() : base(
        "http://localhost:5050",
        "RoadRegistry.BackOffice.Handlers.Sqs.Lambda::RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Function::Handler",
        "aws-lambda-tools-defaults.json") { }
}
