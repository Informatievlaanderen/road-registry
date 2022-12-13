namespace RoadRegistry.Producer.Snapshot.ProjectionHost.RoadSegment;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Syndication.Schema;

public interface IStreetNameCache
{
    Task<StreetNameRecord> GetAsync(int streetNameId, CancellationToken token);
    Task<long> GetMaxPositionAsync(CancellationToken token);
    Task<Dictionary<int, string>> GetStreetNamesByIdAsync(IEnumerable<int> streetNameIds, CancellationToken token);
}
