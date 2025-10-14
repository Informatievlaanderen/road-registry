namespace RoadRegistry.RoadNetwork;

using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

public interface IRoadNetworkRepository
{
    Task<RoadNetwork> Load(Geometry boundingBox, CancellationToken cancellationToken);
    Task Save(RoadNetwork roadNetwork, CancellationToken cancellationToken);
}
