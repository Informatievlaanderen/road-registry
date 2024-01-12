namespace RoadRegistry.BackOffice.Core;

using System.Threading;
using System.Threading.Tasks;

public interface IStreetNames
{
    Task<StreetName> FindAsync(StreetNameId id, CancellationToken ct = default);
}
