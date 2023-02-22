namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenRebuildRoadNetworkSnapshot;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenRebuildRoadNetworkSnapshotWithValidRequest : WhenRebuildRoadNetworkSnapshot<WhenRebuildRoadNetworkSnapshotWithValidRequestFixture>
{
    public WhenRebuildRoadNetworkSnapshotWithValidRequest(WhenRebuildRoadNetworkSnapshotWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
