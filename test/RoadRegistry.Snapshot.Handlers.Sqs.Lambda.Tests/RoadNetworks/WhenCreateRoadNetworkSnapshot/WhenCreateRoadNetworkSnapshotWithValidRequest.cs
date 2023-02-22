namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenCreateRoadNetworkSnapshot;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateRoadNetworkSnapshotWithValidRequest : WhenCreateRoadNetworkSnapshot<WhenCreateRoadNetworkSnapshotWithValidRequestFixture>
{
    public WhenCreateRoadNetworkSnapshotWithValidRequest(WhenCreateRoadNetworkSnapshotWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public void ItShouldSucceed()
    {
        if (Fixture.Exception is not null)
        {
            OutputHelper.WriteLine(Fixture.Exception.ToString());
        }

        Assert.True(Fixture.Result);
    }
}
