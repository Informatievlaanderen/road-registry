namespace RoadRegistry.Sync.StreetNameRegistry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Abstractions;

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
        throw new NotSupportedException();
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
