namespace RoadRegistry.Infrastructure.MartenDb.Store;

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

    public async Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        if (roadSegmentIds.Count == 0)
        {
            return new RoadNetworkIds([], [], []);
        }

        var sql = @$"
WITH ids AS (
	SELECT rs1.id as id1, rs2.id as id2
	FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs1
	JOIN {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs2 ON
		rs1.start_node_id = rs2.start_node_id
        OR rs1.start_node_id = rs2.end_node_id
		OR rs1.end_node_id = rs2.start_node_id
        OR rs1.end_node_id = rs2.end_node_id
	WHERE rs1.id IN ({string.Join(",", roadSegmentIds.Select(x => x.ToInt32()))})
),
flat_ids AS (
    SELECT id1 AS id FROM ids
    UNION
    SELECT id2 AS id FROM ids
)
SELECT rs.id as RoadSegmentId, rs.start_node_id as StartNodeId, rs.end_node_id as EndNodeId, j.id as GradeSeparatedJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
JOIN flat_ids f ON rs.id = f.id
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j ON rs.id = j.upper_road_segment_id OR rs.id = j.lower_road_segment_id
";

        var segments = (await session.Connection.QueryAsync<RoadNetworkTopologySegment>(sql)).ToList();

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

    public async Task<RoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids)
    {
        var roadNodes = await session.LoadManyAsync(ids.RoadNodeIds);
        var roadSegments = await session.LoadManyAsync(ids.RoadSegmentIds);
        var gradeSeparatedJunctions = await session.LoadManyAsync(ids.GradeSeparatedJunctionIds);

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
