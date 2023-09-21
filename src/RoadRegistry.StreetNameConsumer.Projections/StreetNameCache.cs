namespace RoadRegistry.StreetNameConsumer.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Abstractions;
using Microsoft.EntityFrameworkCore;
using Schema;

public class StreetNameCache : IStreetNameCache
{
    private readonly IDbContextFactory<StreetNameConsumerContext> _contextFactory;

    public StreetNameCache(IDbContextFactory<StreetNameConsumerContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<StreetNameCacheItem?> GetAsync(int streetNameId, CancellationToken cancellationToken)
    {
        var items = await GetAsync(new[] { streetNameId }, cancellationToken);
        return items.SingleOrDefault();
    }

    public async Task<long> GetMaxPositionAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        if (!await context.StreetNames.AnyAsync(cancellationToken))
            return -1;

        return await context.StreetNames.MaxAsync(record => record.Offset, cancellationToken);
    }
    
    public async Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        var items = await GetAsync(streetNameIds, cancellationToken);
        
        return items.ToDictionary(x => x.Id, x => x.Name);
    }
    
    private async Task<ICollection<StreetNameCacheItem>> GetAsync(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return (
                await context.StreetNames
                    .Where(record => record.PersistentLocalId > 0)
                    .Where(record => streetNameIds.Contains(record.PersistentLocalId))
                    .ToListAsync(cancellationToken)
            )
            .Select(record => new StreetNameCacheItem
            {
                Id = record.PersistentLocalId,
                NisCode = record.NisCode,
                Name = record.DutchName ?? record.FrenchName ?? record.GermanName ?? record.EnglishName,
                Status = record.StreetNameStatus,
                IsRemoved = record.IsRemoved
            })
            .ToList();
    }
}
