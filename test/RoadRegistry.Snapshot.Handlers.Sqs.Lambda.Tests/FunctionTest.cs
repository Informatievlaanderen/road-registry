using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;

namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests;

using BackOffice.Abstractions.RoadNetworks;
using Newtonsoft.Json;
using RoadNetworks;

public class FunctionTest
{
    [Fact]
    public async Task TestSQSEventLambdaFunction()
    {
        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    Body = JsonConvert.SerializeObject(new CreateRoadNetworkSnapshotSqsRequest
                    {
                        Request = new CreateRoadNetworkSnapshotRequest()
                        {
                            StreamVersion = 1000
                        }
                    })
                }
            }
        };

        var logger = new TestLambdaLogger();
        var context = new TestLambdaContext
        {
            Logger = logger
        };

        var function = new Function();
        await function.Handler(sqsEvent, context);

        Assert.Contains("Processed message foobar", logger.Buffer.ToString());
    }
}
