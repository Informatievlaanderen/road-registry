namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.WhenRebuildRoadNetworkSnapshot.Abstractions;

using Fixtures;
using Xunit.Abstractions;

public abstract class WhenRebuildRoadNetworkSnapshot<TFixture> : IClassFixture<TFixture>
    where TFixture : WhenRebuildRoadNetworkSnapshotFixture
{
    protected TFixture Fixture;
    protected ITestOutputHelper OutputHelper;

    protected WhenRebuildRoadNetworkSnapshot(TFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        OutputHelper = testOutputHelper;
    }
}
