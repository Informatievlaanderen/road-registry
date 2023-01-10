namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Abstractions;
using Microsoft.EntityFrameworkCore;
using Syndication.Schema;

public class StreetNameCache : IStreetNameCache
{
    private readonly Func<SyndicationContext> _contextFactory;

    public StreetNameCache(Func<SyndicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

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

    public async Task<Dictionary<int, string>> GetStreetNameStatusesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory();
        return await context.StreetNames
            .Where(record => record.PersistentLocalId.HasValue)
            .Where(record => streetNameIds.Contains(record.PersistentLocalId.Value))
            .ToDictionaryAsync(
                x => x.PersistentLocalId.Value,
                x => x.StreetNameStatus?.ToString(),
                cancellationToken);
    }
}
