namespace RoadRegistry.Infrastructure.MartenDb.Store;

using Dapper;
using Marten;
using NetTopologySuite.Geometries;
using Projections;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public class RoadNetworkRepository : IRoadNetworkRepository
{
    protected IDocumentStore Store { get; }

    public RoadNetworkRepository(IDocumentStore store)
    {
        Store = store;
    }

    public async Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry geometry, RoadNetworkIds? ids = null, bool onlyV2 = false)
    {
        if (geometry.IsEmpty && (ids is null || ids.IsEmpty))
        {
            throw new ArgumentException("At least a geometry or ids must be specified.");
        }

        var baseSql = @$"
SELECT rs.id as RoadSegmentId, rs.start_node_id as StartNodeId, rs.end_node_id as EndNodeId, j.id as GradeSeparatedJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j ON rs.id = j.upper_road_segment_id OR rs.id = j.lower_road_segment_id
WHERE {(onlyV2 ? "rs.is_v2 AND (j.is_v2 IS NULL OR j.is_v2)" : "TRUE")}";

        var queries = new List<string>();

        if (!geometry.IsEmpty)
        {
            queries.Add($"{baseSql} AND ST_Intersects(rs.geometry, ST_GeomFromText(@wkt, {geometry.SRID}))");
        }

        if (ids is not null)
        {
            if (ids.RoadNodeIds.Any())
            {
                queries.Add($"{baseSql} AND (rs.start_node_id IN ({string.Join(",", ids.RoadNodeIds.Select(x => x.ToInt32()))}) OR rs.end_node_id IN ({string.Join(",", ids.RoadNodeIds.Select(x => x.ToInt32()))}))");
            }

            if (ids.RoadSegmentIds.Any())
            {
                queries.Add($"{baseSql} AND rs.id IN ({string.Join(",", ids.RoadSegmentIds.Select(x => x.ToInt32()))})");
            }

            if (ids.GradeSeparatedJunctionIds.Any())
            {
                queries.Add($"{baseSql} AND j.id IN ({string.Join(",", ids.GradeSeparatedJunctionIds.Select(x => x.ToInt32()))})");
            }
        }

        var sql = string.Join(" UNION ", queries);
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

    public async Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
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
	WHERE rs1.is_v2 AND rs2.is_v2 AND rs1.id IN ({string.Join(",", roadSegmentIds.Select(x => x.ToInt32()))})
),
flat_ids AS (
    SELECT id1 AS id FROM ids
    UNION
    SELECT id2 AS id FROM ids
)
SELECT rs.id as RoadSegmentId, rs.start_node_id as StartNodeId, rs.end_node_id as EndNodeId, j.id as GradeSeparatedJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
JOIN flat_ids f ON rs.id = f.id
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j ON j.is_v2 AND (rs.id = j.upper_road_segment_id OR rs.id = j.lower_road_segment_id)
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

    public async Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
    {
        var roadNodes = await session.LoadManyAsync(ids.RoadNodeIds);
        var roadSegments = await session.LoadManyAsync(ids.RoadSegmentIds);
        var gradeSeparatedJunctions = await session.LoadManyAsync(ids.GradeSeparatedJunctionIds);
        var roadNetwork = await session.Events.AggregateStreamAsync<ScopedRoadNetwork>(StreamKeyFactory.Create(typeof(ScopedRoadNetwork), roadNetworkId));

        return roadNetwork ?? new ScopedRoadNetwork(roadNetworkId, roadNodes, roadSegments, gradeSeparatedJunctions);
    }

    public async Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        session.CorrelationId ??= roadNetwork.RoadNetworkId;
        session.CausationId = commandName;

        SaveEntities(roadNetwork.RoadNodes, session);
        SaveEntities(roadNetwork.RoadSegments, session);
        SaveEntities(roadNetwork.GradeSeparatedJunctions, session);
        foreach (var evt in roadNetwork.GetChanges())
        {
            EnsureEventHasProvenance(evt);
            session.Events.StartStream(roadNetwork.Id, evt);
        }

        await session.SaveChangesAsync(cancellationToken);
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
