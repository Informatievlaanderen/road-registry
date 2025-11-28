namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.RoadNetwork.ValueObjects;

public interface IStreetNames
{
    Task<StreetName> FindAsync(StreetNameLocalId id, CancellationToken ct = default);
}
