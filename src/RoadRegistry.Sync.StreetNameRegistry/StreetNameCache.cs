namespace RoadRegistry.Sync.StreetNameRegistry;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;

public class StreetNameCache : IStreetNameCache
{
    private readonly IDbContextFactory<StreetNameProjectionContext> _streetNameProjectionContextFactory;
    private readonly IDbContextFactory<StreetNameEventConsumerContext> _streetNameEventConsumerContextFactory;

    public StreetNameCache(
        IDbContextFactory<StreetNameProjectionContext> streetNameProjectionContextFactory,
        IDbContextFactory<StreetNameEventConsumerContext> streetNameEventConsumerContextFactory)
    {
        _streetNameProjectionContextFactory = streetNameProjectionContextFactory.ThrowIfNull();
        _streetNameEventConsumerContextFactory = streetNameEventConsumerContextFactory.ThrowIfNull();
    }

    public async Task<StreetNameCacheItem> GetAsync(int streetNameId, CancellationToken cancellationToken)
    {
        var items = await GetAsync(new[] { streetNameId }, cancellationToken);
        return items.SingleOrDefault();
    }

    public async Task<Dictionary<int, int>> GetRenamedIdsAsync(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        //TODO-rik test
        await using var context = await _streetNameEventConsumerContextFactory.CreateDbContextAsync(cancellationToken);

        return (
                await context.RenamedStreetNames
                    .Where(record => streetNameIds.Contains(record.StreetNameLocalId))
                    .ToListAsync(cancellationToken)
            )
            .ToDictionary(x => x.StreetNameLocalId, x => x.DestinationStreetNameLocalId);
    }

    public async Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        var items = await GetAsync(streetNameIds, cancellationToken);
        
        return items.ToDictionary(x => x.Id, x => x.Name);
    }

    public async Task<ICollection<StreetNameCacheItem>> GetAsync(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        await using var context = await _streetNameProjectionContextFactory.CreateDbContextAsync(cancellationToken);

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
