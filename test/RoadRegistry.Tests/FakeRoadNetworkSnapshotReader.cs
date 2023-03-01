namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore.Streams;

public class FakeRoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
{
    public Task<(RoadNetworkSnapshot snapshot, int? version)> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<(RoadNetworkSnapshot snapshot, int? version)>((null, ExpectedVersion.NoStream));
    }

    public Task<int?> ReadSnapshotVersionAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((int?)0);
    }
}
