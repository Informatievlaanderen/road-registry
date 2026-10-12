namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten;
using RoadRegistry.Read.Projections;

public static class DocumentStoreExtensions
{
    // The V2 road segments among the given ones, as the read model knows them. This is what the inwinningsstatus asks
    // about a road segment that takes part in no inwinning: a V2 one is 'compleet' all the same.
    public static Func<IReadOnlyCollection<RoadSegmentId>, CancellationToken, Task<IReadOnlyCollection<RoadSegmentId>>> SelectV2RoadSegmentIds(
        this IDocumentStore store)
    {
        return async (roadSegmentIds, cancellationToken) =>
        {
            await using var session = store.LightweightSession();

            var roadSegments = await session.LoadManyAsync<RoadSegmentReadItem>(cancellationToken, roadSegmentIds.Select(x => x.ToInt32()));

            return roadSegments
                .Where(x => x.IsV2)
                .Select(x => x.RoadSegmentId)
                .ToList();
        };
    }
}
