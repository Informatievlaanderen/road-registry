namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;
using Messages;

public interface IRoadNetworkSnapshotReader
{
    Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken);
}
