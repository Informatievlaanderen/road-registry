namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Tests.RoadNetworks.SnapshotReaderWriter.Fixtures;

using BackOffice.Core;
using BackOffice.Messages;

public class SnapshotReaderFixture : IAsyncLifetime
{
    private readonly IRoadNetworkSnapshotReader _reader;

    public SnapshotReaderFixture(IRoadNetworkSnapshotReader reader)
    {
        _reader = reader;
    }

    public int Version { get; private set; }
    public RoadNetworkSnapshot Snapshot { get; private set; }

    public async Task InitializeAsync()
    {
        (Snapshot, Version) = await _reader.ReadSnapshotAsync(CancellationToken.None);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
