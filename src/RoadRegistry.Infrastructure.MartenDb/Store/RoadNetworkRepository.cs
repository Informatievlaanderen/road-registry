namespace RoadRegistry.Infrastructure.MartenDb.Store;

using System.Data;
using Dapper;
using Marten;
using NetTopologySuite.Geometries;
using Projections;
using RoadNetwork;

public class RoadNetworkRepository : IRoadNetworkRepository
{
    protected IDocumentStore Store { get; }

    public RoadNetworkRepository(IDocumentStore store)
    {
        Store = store;
    }

    public async Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges)
    {
        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry());
        var roadNetwork = await Load(
            session,
            roadNetworkChanges.RoadNodeIds.Union(ids.RoadNodeIds).ToList(),
            roadNetworkChanges.RoadSegmentIds.Union(ids.RoadSegmentIds).ToList(),
            roadNetworkChanges.GradeSeparatedJunctionIds.Union(ids.GradeSeparatedJunctionIds).ToList()
        );

        return roadNetwork;
    }

    private async Task<RoadNetwork> Load(
        IDocumentSession session,
        IReadOnlyCollection<RoadNodeId> roadNodeIds,
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        IReadOnlyCollection<GradeSeparatedJunctionId> gradeSeparatedJunctionIds
    )
    {
        var roadNodes = await session.LoadManyAsync(roadNodeIds);
        var roadSegments = await session.LoadManyAsync(roadSegmentIds);
        var gradeSeparatedJunctions = await session.LoadManyAsync(gradeSeparatedJunctionIds);

        return new RoadNetwork(roadNodes, roadSegments, gradeSeparatedJunctions);
    }

    public async Task Save(RoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        AddChangesToSession(session, roadNetwork, commandName);

        await session.SaveChangesAsync(cancellationToken);
    }

    public void AddChangesToSession(IDocumentSession session, RoadNetwork roadNetwork, string commandName)
    {
        session.CorrelationId ??= Guid.NewGuid().ToString();
        session.CausationId = commandName;

        SaveEntities(roadNetwork.RoadSegments, session);
        SaveEntities(roadNetwork.RoadNodes, session);
        SaveEntities(roadNetwork.GradeSeparatedJunctions, session);
        foreach (var evt in roadNetwork.GetChanges())
        {
            EnsureEventHasProvenance(evt);
            session.Events.Append(roadNetwork.Id, evt);
        }
    }

    public async Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry geometry)
    {
        if (geometry.IsEmpty)
        {
            return new RoadNetworkIds([], [], []);
        }

        var sql = @$"
SELECT rs.id as RoadSegmentId, rs.start_node_id as StartNodeId, rs.end_node_id as EndNodeId, j.id as GradeSeparatedJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j ON rs.id = j.upper_road_segment_id OR rs.id = j.lower_road_segment_id
WHERE ST_Intersects(rs.geometry, ST_GeomFromText(@wkt, {geometry.SRID}))";

        var segments = (await session.Connection.QueryAsync<RoadNetworkTopologySegment>(sql, new { wkt = geometry.AsText() })).ToList();

        return new RoadNetworkIds(
            segments.SelectMany(x => new[] { x.StartNodeId, x.EndNodeId })
                .Distinct()
                .Select(x => new RoadNodeId(x))
                .ToArray(),
            segments.Select(x => x.RoadSegmentId)
                .Distinct()
                .Select(x => new RoadSegmentId(x))
                .ToArray(),
            segments.Where(x => x.GradeSeparatedJunctionId is not null)
                .Select(x => x.GradeSeparatedJunctionId!.Value)
                .Distinct()
                .Select(x => new GradeSeparatedJunctionId(x))
                .ToArray()
        );
    }

    private static void SaveEntities<TKey, TEntity>(IReadOnlyDictionary<TKey, TEntity> entities, IDocumentSession session)
        where TEntity : IMartenAggregateRootEntity
    {
        foreach (var entity in entities.Select(x => x.Value).Where(x => x.HasChanges()))
        {
            foreach (var @event in entity.GetChanges())
            {
                EnsureEventHasProvenance(@event);
                session.Events.AppendOrStartStream(entity.Id, @event);
            }

            session.Store(entity);
        }
    }

    private static void EnsureEventHasProvenance(object @event)
    {
        if (@event is not IMartenEvent martenEvent)
        {
            throw new InvalidOperationException($"Event of type '{@event.GetType().Name}' does not implement '{nameof(IMartenEvent)}'.");
        }

        if (martenEvent.Provenance is null)
        {
            throw new InvalidOperationException($"Event of type '{@event.GetType().Name}' has empty {nameof(martenEvent.Provenance)}.");
        }
    }

    private sealed record RoadNetworkTopologySegment(int RoadSegmentId, int StartNodeId, int EndNodeId, int? GradeSeparatedJunctionId);
}
