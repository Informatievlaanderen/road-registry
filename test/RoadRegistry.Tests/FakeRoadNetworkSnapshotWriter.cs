namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;

public class FakeRoadNetworkSnapshotWriter : IRoadNetworkSnapshotWriter
{
    public Task SetHeadToVersion(int version, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}