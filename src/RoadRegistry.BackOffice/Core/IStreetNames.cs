namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;

public interface IStreetNames
{
    Task<StreetName> FindAsync(StreetNameLocalId id, CancellationToken ct = default);
}
