namespace RoadRegistry.RoadNetwork;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkRepository
{
    Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges);
    Task Save(RoadNetwork roadNetwork, CancellationToken cancellationToken);
}
