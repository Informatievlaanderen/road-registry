namespace RoadRegistry.RoadNetwork;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public interface IRoadNetworkRepository
{
    Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges);
    Task Save(RoadNetwork roadNetwork, string commandName, Provenance provenance, CancellationToken cancellationToken);
}
