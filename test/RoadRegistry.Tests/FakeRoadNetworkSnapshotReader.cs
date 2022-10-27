namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore.Streams;

public class FakeRoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
{
    public Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
    {
        return Task.FromResult<(RoadNetworkSnapshot snapshot, int version)>((null, ExpectedVersion.NoStream));
    }
}