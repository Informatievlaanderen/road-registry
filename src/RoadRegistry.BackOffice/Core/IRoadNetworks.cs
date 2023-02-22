namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworks
{
    Task<RoadNetwork> Get(bool restoreSnapshot, int maximumStreamVersion, CancellationToken cancellationToken);
    Task<RoadNetwork> Get(CancellationToken cancellationToken);
    Task<(RoadNetwork, int)> GetWithVersion(CancellationToken cancellationToken);
    Task<(RoadNetwork, int)> GetWithVersion(bool restoreSnapshot, CancellationToken cancellationToken);
}
