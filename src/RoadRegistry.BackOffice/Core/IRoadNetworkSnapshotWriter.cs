namespace RoadRegistry.BackOffice.Core;

using Messages;
using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkSnapshotWriter
{
    Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken);
}
