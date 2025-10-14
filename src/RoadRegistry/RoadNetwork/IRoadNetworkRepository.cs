namespace RoadRegistry.RoadNetwork;

using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

public interface IRoadNetworkRepository
{
    Task<RoadNetwork> GetScopedRoadNetwork(Geometry boundingBox, CancellationToken cancellationToken);
}
