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

    public async Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
    {
        if ((geometry is null || geometry.IsEmpty) && (ids is null || ids.IsEmpty))
        {
            throw new ArgumentException("At least a geometry or ids must be specified.");
        }

        var sql = $@"
WITH
input AS (
  SELECT
    -- Optional inputs (pass NULL or empty array when not used)
    ST_SetSRID(ST_GeomFromText(@wkt), @srid)       AS boundary_geom,               -- may be NULL
    @roadsegmentids::bigint[]                      AS roadsegmentids,              -- may be NULL/empty
    @roadnodeids::bigint[]                         AS roadnodeids,                 -- may be NULL/empty
    @gradeseparatedjunctionids::bigint[]           AS gradeseparatedjunctionids    -- may be NULL/empty
    @gradejunctionids::bigint[]                    AS gradejunctionids             -- may be NULL/empty
),

/* Seed segments = UNION of:
   - spatial hits
   - explicit roadsegmentids
   - segments touched by roadnodeids (start/end)
   - segments referenced by explicit gradeseparatedjunctionids (upper/lower)
   - segments referenced by explicit gradejunctionids (1/2)
*/
seed_segments AS (
  -- spatial
  SELECT rs.id
  FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
  CROSS JOIN input i
  WHERE i.boundary_geom IS NOT NULL
    AND ST_Intersects(rs.geometry, i.boundary_geom)

  UNION

  -- roadsegmentids
  SELECT rs.id
  FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
  CROSS JOIN input i
  WHERE i.roadsegmentids IS NOT NULL
    AND cardinality(i.roadsegmentids) > 0
    AND rs.id = ANY(i.roadsegmentids)

  UNION

  -- roadnodeids
  SELECT rs.id
  FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
  CROSS JOIN input i
  WHERE i.roadnodeids IS NOT NULL
    AND cardinality(i.roadnodeids) > 0
    AND (rs.start_node_id = ANY(i.roadnodeids) OR rs.end_node_id = ANY(i.roadnodeids))

  UNION

  -- segments implied by gradeseparatedjunctionids
  SELECT j.upper_road_segment_id
  FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j
  CROSS JOIN input i
  WHERE i.gradeseparatedjunctionids IS NOT NULL
    AND cardinality(i.gradeseparatedjunctionids) > 0
    AND j.id = ANY(i.gradeseparatedjunctionids)

  UNION

  SELECT j.lower_road_segment_id
  FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j
  CROSS JOIN input i
  WHERE i.gradeseparatedjunctionids IS NOT NULL
    AND cardinality(i.gradeseparatedjunctionids) > 0
    AND j.id = ANY(i.gradeseparatedjunctionids)

  UNION

  -- segments implied by gradejunctionids
  SELECT j.road_segment_id_1
  FROM {RoadNetworkTopologyProjection.GradeJunctionsTableName} j
  CROSS JOIN input i
  WHERE i.gradejunctionids IS NOT NULL
    AND cardinality(i.gradejunctionids) > 0
    AND j.id = ANY(i.gradejunctionids)

  UNION

  SELECT j.road_segment_id_2
  FROM {RoadNetworkTopologyProjection.GradeJunctionsTableName} j
  CROSS JOIN input i
  WHERE i.gradejunctionids IS NOT NULL
    AND cardinality(i.gradejunctionids) > 0
    AND j.id = ANY(i.gradejunctionids)
),

/* Seed gradeseparatedjunctions = UNION of:
   - explicit gradeseparatedjunctionids
   - junctions touching any seed segment (using UNION ALL branches; dedup after)
*/
seed_gradeseparatedjunctions_raw AS (
  -- explicit gradeseparatedjunctionids
  SELECT j.id, j.upper_road_segment_id, j.lower_road_segment_id
  FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j
  CROSS JOIN input i
  WHERE i.gradeseparatedjunctionids IS NOT NULL
    AND cardinality(i.gradeseparatedjunctionids) > 0
    AND j.id = ANY(i.gradeseparatedjunctionids)

  UNION ALL

  -- junctions where upper segment is in seed
  SELECT j.id, j.upper_road_segment_id, j.lower_road_segment_id
  FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j
  WHERE j.upper_road_segment_id IN (SELECT id FROM seed_segments)

  UNION ALL

  -- junctions where lower segment is in seed
  SELECT j.id, j.upper_road_segment_id, j.lower_road_segment_id
  FROM {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j
  WHERE j.lower_road_segment_id IN (SELECT id FROM seed_segments)
),
seed_gradeseparatedjunctions AS (
  SELECT DISTINCT id, upper_road_segment_id, lower_road_segment_id
  FROM seed_gradeseparatedjunctions_raw
),

/* Seed gradejunctions = UNION of:
   - explicit gradejunctionids
   - junctions touching any seed segment (using UNION ALL branches; dedup after)
*/
seed_gradejunctions_raw AS (
  -- explicit gradejunctionids
  SELECT j.id, j.road_segment_id_1, j.road_segment_id_2
  FROM {RoadNetworkTopologyProjection.GradeJunctionsTableName} j
  CROSS JOIN input i
  WHERE i.gradejunctionids IS NOT NULL
    AND cardinality(i.gradejunctionids) > 0
    AND j.id = ANY(i.gradejunctionids)

  UNION ALL

  -- junctions where segment 1 is in seed
  SELECT j.id, j.road_segment_id_1, j.road_segment_id_2
  FROM {RoadNetworkTopologyProjection.GradeJunctionsTableName} j
  WHERE j.road_segment_id_1 IN (SELECT id FROM seed_segments)

  UNION ALL

  -- junctions where lower segment is in seed
  SELECT j.id, j.road_segment_id_1, j.road_segment_id_2
  FROM {RoadNetworkTopologyProjection.GradeJunctionsTableName} j
  WHERE j.road_segment_id_2 IN (SELECT id FROM seed_segments)
),
seed_gradejunctions AS (
  SELECT DISTINCT id, road_segment_id_1, road_segment_id_2
  FROM seed_gradejunctions_raw
),

-- Expanded segments: always include both ends of seed junctions (even if outside spatial boundary)
expanded_segments AS (
  SELECT id FROM seed_segments
  UNION
  SELECT upper_road_segment_id FROM seed_gradeseparatedjunctions
  UNION
  SELECT lower_road_segment_id FROM seed_gradeseparatedjunctions
  UNION
  SELECT road_segment_id_1 FROM seed_gradejunctions
  UNION
  SELECT road_segment_id_2 FROM seed_gradejunctions
),

-- Segment<->gradeseparatedjunction link rows, but ONLY for seed gradeseparatedjunctions
segment_gradeseparatedjunction_links AS (
  SELECT upper_road_segment_id AS road_segment_id, id AS gradeseparatedjunctionid
  FROM seed_gradeseparatedjunctions
  UNION ALL
  SELECT lower_road_segment_id AS road_segment_id, id AS gradeseparatedjunctionid
  FROM seed_gradeseparatedjunctions
),

-- Segment<->gradejunction link rows, but ONLY for seed gradejunctions
segment_gradejunction_links AS (
  SELECT road_segment_id_1 AS road_segment_id, id AS gradejunctionid
  FROM seed_gradejunctions
  UNION ALL
  SELECT road_segment_id_2 AS road_segment_id, id AS gradejunctionid
  FROM seed_gradejunctions
)

SELECT
  rs.id            AS RoadSegmentId,
  rs.start_node_id AS StartNodeId,
  rs.end_node_id   AS EndNodeId,
  sgsjl.gradeseparatedjunctionid AS GradeSeparatedJunctionId,
  sgjl.gradejunctionid AS GradeJunctionId
FROM expanded_segments es
JOIN {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
  ON rs.id = es.id
LEFT JOIN segment_gradeseparatedjunction_links sgsjl
  ON sgsjl.road_segment_id = rs.id
LEFT JOIN segment_gradejunction_links sgjl
  ON sgjl.road_segment_id = rs.id
";
        var segments = (await session.Connection.QueryAsync<RoadNetworkTopologySegment>(sql,
            new
            {
                wkt = geometry?.AsText(),
                srid = geometry?.SRID,
                roadsegmentids = ids?.RoadSegmentIds.Count > 0 ? ids.RoadSegmentIds.Select(x => x.ToInt32()).ToArray() : null,
                roadnodeids = ids?.RoadNodeIds.Count > 0 ? ids.RoadNodeIds.Select(x => x.ToInt32()).ToArray() : null,
                gradeseparatedjunctionids = ids?.GradeSeparatedJunctionIds.Count > 0 ? ids.GradeSeparatedJunctionIds.Select(x => x.ToInt32()).ToArray() : null,
                gradejunctionids = ids?.GradeJunctionIds.Count > 0 ? ids.GradeJunctionIds.Select(x => x.ToInt32()).ToArray() : null
            })).ToList();

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
                .ToArray(),
            segments.Where(x => x.GradeJunctionId is not null)
                .Select(x => x.GradeJunctionId!.Value)
                .Distinct()
                .Select(x => new GradeJunctionId(x))
                .ToArray()
        );
    }

    public async Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        if (roadSegmentIds.Count == 0)
        {
            return new RoadNetworkIds([], [], [], []);
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
SELECT rs.id as RoadSegmentId, rs.start_node_id as StartNodeId, rs.end_node_id as EndNodeId, gsj.id as GradeSeparatedJunctionId, gj.id as GradeJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
JOIN flat_ids f ON rs.id = f.id
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} gsj ON gsj.is_v2 AND (rs.id = gsj.upper_road_segment_id OR rs.id = gsj.lower_road_segment_id)
LEFT JOIN {RoadNetworkTopologyProjection.GradeJunctionsTableName} gj ON gj.is_v2 AND (rs.id = gj.road_segment_id_1 OR rs.id = gj.road_segment_id_2)
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
                .ToArray(),
            segments.Where(x => x.GradeJunctionId is not null)
                .Select(x => x.GradeJunctionId!.Value)
                .Distinct()
                .Select(x => new GradeJunctionId(x))
                .ToArray()
        );
    }

    public async Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
    {
        var roadNodes = await session.LoadManyAsync(ids.RoadNodeIds);
        var roadSegments = await session.LoadManyAsync(ids.RoadSegmentIds);
        var gradeSeparatedJunctions = await session.LoadManyAsync(ids.GradeSeparatedJunctionIds);
        var gradeJunctions = await session.LoadManyAsync(ids.GradeJunctionIds);
        var roadNetwork = await session.Events.AggregateStreamAsync<ScopedRoadNetwork>(StreamKeyFactory.Create(typeof(ScopedRoadNetwork), roadNetworkId));

        return roadNetwork ?? new ScopedRoadNetwork(roadNetworkId, roadNodes, roadSegments, gradeSeparatedJunctions, gradeJunctions);
    }

    public async Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        session.CorrelationId ??= roadNetwork.RoadNetworkId;
        session.CausationId = commandName;

        SaveEntities(roadNetwork.RoadNodes, session);
        SaveEntities(roadNetwork.RoadSegments, session);
        SaveEntities(roadNetwork.GradeSeparatedJunctions, session);
        SaveEntities(roadNetwork.GradeJunctions, session);
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

    private sealed record RoadNetworkTopologySegment(int RoadSegmentId, int StartNodeId, int EndNodeId, int? GradeSeparatedJunctionId, int? GradeJunctionId);
}
