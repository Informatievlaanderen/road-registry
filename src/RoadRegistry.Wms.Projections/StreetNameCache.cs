namespace RoadRegistry.Wms.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Syndication.Schema;

public class StreetNameCache : IStreetNameCache
{
    private readonly IDbContextFactory<SyndicationContext> _contextFactory;

    public StreetNameCache(IDbContextFactory<SyndicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<StreetNameRecord> GetAsync(int streetNameId, CancellationToken token)
    {
        using (var context = await _contextFactory.CreateDbContextAsync(token))
        {
            return await context.StreetNames
                .SingleOrDefaultAsync(record => record.PersistentLocalId == streetNameId, token);
        }
    }

    public async Task<long> GetMaxPositionAsync(CancellationToken token)
    {
        using (var context = await _contextFactory.CreateDbContextAsync(token))
        {
            if (!await context.StreetNames.AnyAsync(token))
                return -1;

            return await context.StreetNames.MaxAsync(record => record.Position, token);
        }
    }

    public async Task<Dictionary<int, string>> GetStreetNamesByIdAsync(IEnumerable<int> streetNameIds, CancellationToken token)
    {
        using (var context = await _contextFactory.CreateDbContextAsync(token))
        {
            return await context.StreetNames
                .Where(record => record.PersistentLocalId.HasValue)
                .Where(record => streetNameIds.Contains(record.PersistentLocalId.Value))
                .ToDictionaryAsync(
                    x => x.PersistentLocalId.Value,
                    x => x.DutchNameWithHomonymAddition,
                    token);
        }
    }
}
