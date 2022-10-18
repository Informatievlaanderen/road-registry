namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;
using Messages;

public interface IRoadNetworkSnapshotWriter
{
    Task SetHeadToVersion(int version, CancellationToken cancellationToken);
    Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken);
}