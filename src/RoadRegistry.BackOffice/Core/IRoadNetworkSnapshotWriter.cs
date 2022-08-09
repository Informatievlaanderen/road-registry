namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;
using Messages;

public interface IRoadNetworkSnapshotWriter
{
    Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken);
    Task SetHeadToVersion(int version, CancellationToken cancellationToken);
}
