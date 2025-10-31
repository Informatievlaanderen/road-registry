namespace RoadRegistry.Infrastructure.MartenDb;

using BackOffice;
using Marten;
using GradeSeparatedJunction;
using Marten.Events;
using RoadNode;
using RoadSegment;
using RoadSegment.ValueObjects;

public static class SessionExtensions
{
    public static void AppendOrStartStream(this IEventStoreOperations operations, string streamKey, object @event)
    {
        if (@event is ICreatedEvent)
        {
            operations.StartStream(streamKey, @event);
        }
        else
        {
            operations.Append(streamKey, @event);
        }
    }

    public static async Task<IReadOnlyList<RoadNode>> LoadRoadNodesAsync(this IDocumentSession session, IEnumerable<RoadNodeId> ids)
    {
        return await session.LoadManyEntitiesAsync<RoadNode, RoadNodeId>(ids);
    }

    public static async Task<IReadOnlyList<RoadSegment>> LoadRoadSegmentsAsync(this IDocumentSession session, IEnumerable<RoadSegmentId> ids)
    {
        return await session.LoadManyEntitiesAsync<RoadSegment, RoadSegmentId>(ids);
    }

    public static async Task<IReadOnlyList<GradeSeparatedJunction>> LoadGradeSeparatedJunctionAsync(this IDocumentSession session, IEnumerable<GradeSeparatedJunctionId> ids)
    {
        return await session.LoadManyEntitiesAsync<GradeSeparatedJunction, GradeSeparatedJunctionId>(ids);
    }

    private static async Task<IReadOnlyList<TEntity>> LoadManyEntitiesAsync<TEntity, TIdentifier>(this IDocumentSession session, IEnumerable<TIdentifier> ids)
        where TEntity : notnull
    {
        return await session.LoadManyAsync<TEntity>(ids.Select(x => StreamKeyFactory.Create(typeof(TEntity), x)));
    }
}
