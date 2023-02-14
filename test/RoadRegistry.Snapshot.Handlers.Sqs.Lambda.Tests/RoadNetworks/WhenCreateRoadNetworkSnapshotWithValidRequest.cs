namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateRoadNetworkSnapshotWithValidRequest : WhenCreateRoadNetworkSnapshot<WhenCreateRoadNetworkSnapshotWithValidRequestFixture>
{
    public WhenCreateRoadNetworkSnapshotWithValidRequest(WhenCreateRoadNetworkSnapshotWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    // TODO-Jan 
    [Fact(Skip = "Setup before test run")]
    public void ItShouldSucceed()
    {
        if (Fixture.Exception is not null)
        {
            OutputHelper.WriteLine(Fixture.Exception.ToString());
        }

        Assert.True(Fixture.Result);
    }
}
