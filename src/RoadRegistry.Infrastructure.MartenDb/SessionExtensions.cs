namespace RoadRegistry.Infrastructure.MartenDb;

using BackOffice;
using Marten;
using GradeSeparatedJunction;
using JasperFx.Events;
using Marten.Events;
using RoadNode;
using RoadSegment;
using RoadSegment.ValueObjects;

public static class SessionExtensions
{
    public static StreamAction AppendOrStartStream(this IEventStoreOperations operations, string streamKey, object @event)
    {
        if (@event is ICreatedEvent)
        {
            return operations.StartStream(streamKey, @event);
        }

        return operations.Append(streamKey, @event);
    }

    public static async Task<RoadNode?> LoadAsync(this IDocumentSession session, RoadNodeId id)
    {
        var result = await session.LoadManyAsync([id]);
        return result.SingleOrDefault();
    }
    public static async Task<IReadOnlyList<RoadNode>> LoadManyAsync(this IDocumentSession session, IEnumerable<RoadNodeId> ids)
    {
        return await session.LoadManyEntitiesAsync<RoadNode, RoadNodeId>(ids);
    }

    public static async Task<RoadSegment?> LoadAsync(this IDocumentSession session, RoadSegmentId id)
    {
        var result = await session.LoadManyAsync([id]);
        return result.SingleOrDefault();
    }
    public static async Task<IReadOnlyList<RoadSegment>> LoadManyAsync(this IDocumentSession session, IEnumerable<RoadSegmentId> ids)
    {
        return await session.LoadManyEntitiesAsync<RoadSegment, RoadSegmentId>(ids);
    }

    public static async Task<GradeSeparatedJunction?> LoadAsync(this IDocumentSession session, GradeSeparatedJunctionId id)
    {
        var result = await session.LoadManyAsync([id]);
        return result.SingleOrDefault();
    }
    public static async Task<IReadOnlyList<GradeSeparatedJunction>> LoadManyAsync(this IDocumentSession session, IEnumerable<GradeSeparatedJunctionId> ids)
    {
        return await session.LoadManyEntitiesAsync<GradeSeparatedJunction, GradeSeparatedJunctionId>(ids);
    }

    private static async Task<IReadOnlyList<TEntity>> LoadManyEntitiesAsync<TEntity, TIdentifier>(this IDocumentSession session, IEnumerable<TIdentifier> ids)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        var streamKeys = ids.Select(x => StreamKeyFactory.Create(typeof(TEntity), x)).ToList();
        if (!streamKeys.Any())
        {
            return [];
        }

        var aggregates = (await session.LoadManyAsync<TEntity>(streamKeys)).ToList();

        foreach (var streamKey in streamKeys.Where(x => aggregates.All(snapshot => snapshot.Id != x)))
        {
            var aggregate = await session.Events.AggregateStreamAsync<TEntity>(streamKey);
            if (aggregate is not null)
            {
                aggregate.RequestToSaveSnapshot();

                aggregates.Add(aggregate);
            }
        }

        return aggregates.AsReadOnly();
    }
}
