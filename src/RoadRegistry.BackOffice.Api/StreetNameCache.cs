namespace RoadRegistry.BackOffice.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Microsoft.EntityFrameworkCore;
using Syndication.Schema;

public class StreetNameCache : IStreetNameCache
{
    public StreetNameCache(Func<SyndicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    private readonly Func<SyndicationContext> _contextFactory;

    public async Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory();
        return await context.StreetNames
            .Where(record => record.PersistentLocalId.HasValue)
            .Where(record => streetNameIds.Contains(record.PersistentLocalId.Value))
            .ToDictionaryAsync(
                x => x.PersistentLocalId.Value,
                x => x.DutchNameWithHomonymAddition,
                cancellationToken);
    }
}
