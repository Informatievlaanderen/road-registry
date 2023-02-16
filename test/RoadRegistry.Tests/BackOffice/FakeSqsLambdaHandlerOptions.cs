namespace RoadRegistry.Tests.BackOffice;

using Hosts;

public class FakeSqsLambdaHandlerOptions : SqsLambdaHandlerOptions
{
    public FakeSqsLambdaHandlerOptions()
    {
        DetailUrl = "http://base/{0}";
    }
}
