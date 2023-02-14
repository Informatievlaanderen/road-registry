namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworks
{
    Task<RoadNetwork> Get(int maximumStreamVersion, CancellationToken ct = default);
    Task<RoadNetwork> Get(CancellationToken ct = default);
    Task<(RoadNetwork, int)> GetWithVersion(CancellationToken ct = default);
}
