namespace RoadRegistry.Infrastructure.MartenDb.Store;

using Dapper;
using JasperFx.Events;
using Marten;
using NetTopologySuite.Geometries;
using Projections;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public class RoadNetworkRepository : IRoadNetworkRepository
{
    // Which road segments an extract is made of. An extract is the road network as it is on the ground and as it
    // can be handed back: gepland and gerealiseerd are the only two statuses an inwinning archive is read back in
    // (RoadSegmentFeatureCompareFeatureReader.ReadStatus), so anything else - gehistoreerd above all, which says
    // the road was opgeheven, samengevoegd or gesplitst - is left out, and so is everything removed.
    //
    // A V1 road segment has no V2 status and is taken whatever state it is in; V1 removals delete the topology row,
    // so a removed one is not there to begin with.
    //
    // The road nodes and the crossings come along per road segment, and each is left out when it is marked removed
    // itself: a crossing a merger or a split did away with still hangs off a road segment that is very much there.
    private static readonly string[] ExtractRoadSegmentStatuses =
    [
        RoadSegmentStatusV2.Gepland.ToString(),
        RoadSegmentStatusV2.Gerealiseerd.ToString()
    ];

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
    @gradeseparatedjunctionids::bigint[]           AS gradeseparatedjunctionids,   -- may be NULL/empty
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
)

SELECT
  rs.id                     AS RoadSegmentId,
  rs.start_node_id          AS StartNodeId,
  rs.end_node_id            AS EndNodeId,
  gradeseparatedjunction.id AS GradeSeparatedJunctionId,
  gradejunction.id          AS GradeJunctionId
FROM seed_segments es
JOIN {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
  ON rs.id = es.id
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} gradeseparatedjunction
  ON gradeseparatedjunction.lower_road_segment_id = rs.id OR gradeseparatedjunction.upper_road_segment_id = rs.id
LEFT JOIN {RoadNetworkTopologyProjection.GradeJunctionsTableName} gradejunction
  ON gradejunction.road_segment_id_1 = rs.id OR gradejunction.road_segment_id_2 = rs.id
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
                .Where(x => x.HasValue)
                .Distinct()
                .Select(x => new RoadNodeId(x!.Value))
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

    public async Task<RoadNetworkIds> GetUnderlyingIdsForExtract(IDocumentSession session, Geometry geometry)
    {
        if (geometry is null || geometry.IsEmpty)
        {
            throw new ArgumentException("Geometry must be specified.");
        }

        var sql = $@"
SELECT
  rs.id                     AS RoadSegmentId,
  startnode.id              AS StartNodeId,
  endnode.id                AS EndNodeId,
  gradeseparatedjunction.id AS GradeSeparatedJunctionId,
  gradejunction.id          AS GradeJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
LEFT JOIN {RoadNetworkTopologyProjection.RoadNodesTableName} startnode
  ON startnode.id = rs.start_node_id AND NOT startnode.is_removed
LEFT JOIN {RoadNetworkTopologyProjection.RoadNodesTableName} endnode
  ON endnode.id = rs.end_node_id AND NOT endnode.is_removed
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} gradeseparatedjunction
  ON (gradeseparatedjunction.lower_road_segment_id = rs.id OR gradeseparatedjunction.upper_road_segment_id = rs.id)
  AND NOT gradeseparatedjunction.is_removed
LEFT JOIN {RoadNetworkTopologyProjection.GradeJunctionsTableName} gradejunction
  ON (gradejunction.road_segment_id_1 = rs.id OR gradejunction.road_segment_id_2 = rs.id)
  AND NOT gradejunction.is_removed
WHERE ST_Intersects(rs.geometry, ST_SetSRID(ST_GeomFromText(@wkt), @srid))
  AND NOT rs.is_removed
  AND (NOT rs.is_v2 OR rs.status = ANY(@statuses))
";
        var segments = (await session.Connection.QueryAsync<RoadNetworkTopologySegment>(sql,
            new
            {
                wkt = geometry.AsText(),
                srid = geometry.SRID,
                statuses = ExtractRoadSegmentStatuses
            })).ToList();

        return new RoadNetworkIds(
            segments.SelectMany(x => new[] { x.StartNodeId, x.EndNodeId })
                .Where(x => x.HasValue)
                .Distinct()
                .Select(x => new RoadNodeId(x!.Value))
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
WITH requested AS (
    SELECT rs.id, rs.start_node_id, rs.end_node_id
    FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
    WHERE rs.is_v2 AND rs.id IN ({string.Join(",", roadSegmentIds.Select(x => x.ToInt32()))})
),
flat_ids AS (
    -- The road segments the caller named, knotted into the road network or not. An ingeschetst road segment has no
    -- road nodes, and the join below compares them, so it answers for no road segment without them - not even with
    -- itself, NULL being equal to nothing in SQL. Asked for on its own it used to load an empty road network, and
    -- the action then had nothing to work on and said nothing about it.
    SELECT id FROM requested
    UNION
    -- And every road segment meeting one of them at a road node: what an action does to the one is a statement
    -- about the other.
    SELECT other.id
    FROM requested
    JOIN {RoadNetworkTopologyProjection.RoadSegmentsTableName} other ON other.is_v2 AND (
        requested.start_node_id = other.start_node_id
        OR requested.start_node_id = other.end_node_id
        OR requested.end_node_id = other.start_node_id
        OR requested.end_node_id = other.end_node_id
    )
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
                .Where(x => x.HasValue)
                .Distinct()
                .Select(x => new RoadNodeId(x!.Value))
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

        return new ScopedRoadNetwork(roadNetworkId, roadNodes, roadSegments, gradeSeparatedJunctions, gradeJunctions);
    }

    public async Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        Save(session, roadNetwork, commandName);

        await session.SaveChangesAsync(cancellationToken);
    }

    public void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
    {
        session.CorrelationId ??= roadNetwork.RoadNetworkId;
        session.CausationId = commandName;

        var usedEventOrdinals = new HashSet<long>();

        SaveEntities(roadNetwork.RoadNodes.Values, session, usedEventOrdinals);
        SaveEntities(roadNetwork.RoadSegments.Values, session, usedEventOrdinals);
        SaveEntities(roadNetwork.GradeSeparatedJunctions.Values, session, usedEventOrdinals);
        SaveEntities(roadNetwork.GradeJunctions.Values, session, usedEventOrdinals);

        SaveEntityEvents(roadNetwork, session, usedEventOrdinals);
    }

    private void SaveEntities<TEntity>(IEnumerable<TEntity> entities, IDocumentOperations session, HashSet<long> usedEventOrdinals)
        where TEntity : IMartenAggregateRootEntity
    {
        foreach (var entity in entities.Where(x => x.HasChanges()))
        {
            SaveEntityEvents(entity, session, usedEventOrdinals);

            session.Store(entity);
        }
    }

    private void SaveEntityEvents<TEntity>(TEntity entity, IDocumentOperations session, HashSet<long> usedEventOrdinals)
        where TEntity : IMartenAggregateRootEntity
    {
        foreach (var recorded in entity.GetRecordedChanges())
        {
            if (!usedEventOrdinals.Add(recorded.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Duplicate event ordinal(s) [{recorded.Ordinal}] while saving road network. " +
                    $"An aggregate emitted events without the change's {nameof(IEventOrdinalProvider)} attached (fell back to {nameof(EventOrdinalProvider)}.None).");
            }

            EnsureEventHasProvenance(recorded.Event);
            var action = session.Events.AppendOrStartStream(entity.Id, recorded.Event);
            SetOrdinalHeader(action, recorded.Ordinal);
        }
    }

    // Stamps the emission ordinal onto the just-appended event as a Marten header so the read projection can
    // replay a correlation's events in true emission order rather than by seq_id (which Marten does not preserve
    // across stream appends vs. new-stream creations). See EventOrdinal / IEventOrdinalProvider.
    private static void SetOrdinalHeader(StreamAction action, long ordinal)
    {
        if (action.Events.Count == 0)
        {
            return;
        }

        action.Events[^1].SetHeader(EventOrdinal.HeaderKey, ordinal);
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

    private sealed record RoadNetworkTopologySegment(int RoadSegmentId, int? StartNodeId, int? EndNodeId, int? GradeSeparatedJunctionId, int? GradeJunctionId);
}


public sealed record IdempotentSession(string Id);
