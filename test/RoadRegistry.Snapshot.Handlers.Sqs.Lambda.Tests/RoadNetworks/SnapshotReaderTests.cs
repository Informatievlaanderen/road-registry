namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks;

using Fixtures;

public class SnapshotReaderTests : IClassFixture<SnapshotReaderFixture>
{
    private readonly SnapshotReaderFixture _fixture;

    public SnapshotReaderTests(SnapshotReaderFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ItShouldSucceed()
    {
        Assert.NotEqual(0, _fixture.Version);
        Assert.NotNull(_fixture.Snapshot);
        Assert.NotEqual(default, _fixture.Snapshot);
    }
}
