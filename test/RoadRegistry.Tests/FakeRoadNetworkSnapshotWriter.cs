namespace RoadRegistry;

using BackOffice.Core;
using BackOffice.Messages;

public class FakeRoadNetworkSnapshotWriter : IRoadNetworkSnapshotWriter
{
    public Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task SetHeadToVersion(int version, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
