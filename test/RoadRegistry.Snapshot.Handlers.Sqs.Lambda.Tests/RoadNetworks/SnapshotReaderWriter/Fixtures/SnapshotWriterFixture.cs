namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.SnapshotReaderWriter.Fixtures;

using BackOffice.Core;
using BackOffice.Messages;

public class SnapshotWriterFixture : IAsyncLifetime
{
    private readonly IRoadNetworkSnapshotWriter _writer;

    public SnapshotWriterFixture(IRoadNetworkSnapshotWriter writer)
    {
        _writer = writer;
    }

    public int Version { get; private set; }
    public RoadNetworkSnapshot Snapshot { get; private set; }

    public async Task InitializeAsync()
    {
        var network = ImmutableRoadNetworkView.Empty;

        Snapshot = network.TakeSnapshot();
        Version = 0;

        await _writer.WriteSnapshot(Snapshot, Version, CancellationToken.None);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}