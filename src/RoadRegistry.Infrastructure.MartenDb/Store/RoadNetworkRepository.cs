namespace RoadRegistry.Infrastructure.MartenDb.Store;

using GradeJunction;
using GradeSeparatedJunction;
using JasperFx.Events;
using Marten;
using Marten.Linq.MatchesSql;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadSegment;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public class RoadNetworkRepository : IRoadNetworkRepository
{
    // Which road segments an extract is made of. An extract is the road network as it is on the ground and as it
    // can be handed back: gepland and gerealiseerd are the only two statuses an inwinning archive is read back in
    // (RoadSegmentFeatureCompareFeatureReader.ReadStatus), so anything else - gehistoreerd above all, which says
    // the road was opgeheven, samengevoegd or gesplitst - is left out, and so is everything removed.
    //
    // A road segment that is not migrated yet is taken whatever its status: the one the aggregate carries was only
    // derived from its geometry draw method when it was carried over from the legacy road network.
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

    // A road network is loaded in one round trip: one batch, with a query per kind of document. Each query takes the
    // documents that belong to the road segments in scope, and works out for itself which road segments those are -
    // a subquery on the columns the 20261103120000_add_topology_columns_to_aggregate_documents migration generates on
    // the aggregate documents. The scope is found among the documents themselves, so every entity in it has a
    // document to load.
    public async Task<ScopedRoadNetwork> LoadUnderlying(IDocumentSession session, ScopedRoadNetworkId roadNetworkId, Geometry? geometry = null, RoadNetworkIds? ids = null)
    {
        if ((geometry is null || geometry.IsEmpty) && (ids is null || ids.IsEmpty))
        {
            throw new ArgumentException("At least a geometry or ids must be specified.");
        }

        // A road node is reached through the road segments that point at it, which leaves out the one case where a
        // caller names a road node that nothing points at any more: an inwinning that merges a road across a
        // municipality border removes the road node the two met at, and leaves the road segment it took over
        // retired and without road nodes. The second municipality's upload still names that road node, and the
        // migration has to be handed it - as removed, so it can cut the upload back against what the neighbour
        // already collected, rather than refuse the upload over a road node it cannot find.
        //
        // The document is still there: a removal only marks it.
        var namedRoadNodeIds = ids?.RoadNodeIds.Select(x => x.ToInt32()).ToArray() ?? [];

        return await Load(session, roadNetworkId, RoadSegmentsInScope(geometry, ids), namedRoadNodeIds, migratedGradeSeparatedJunctionsOnly: false);
    }

    // The road segments a geometry and ids put in scope, as a subquery selecting their id. It is the UNION of:
    // - the road segments intersecting the geometry
    // - the road segments named
    // - the road segments starting or ending at a road node named
    // - the road segments a grade separated junction named lies on (upper and lower)
    // - the road segments a grade junction named lies on
    // Only the parts that were asked for are written out, so no parameter is ever NULL.
    private static RoadSegmentScope RoadSegmentsInScope(Geometry? geometry, RoadNetworkIds? ids)
    {
        var seeds = new List<string>();
        var parameters = new List<object>();

        if (geometry is not null && !geometry.IsEmpty)
        {
            seeds.Add($"SELECT s.road_segment_id FROM {RoadNetworkTopologyTables.RoadSegments} s WHERE ST_Intersects(s.geometry, ST_SetSRID(ST_GeomFromText(?), ?))");
            parameters.Add(geometry.AsText());
            parameters.Add(geometry.SRID);
        }

        if (ids?.RoadSegmentIds.Count > 0)
        {
            seeds.Add($"SELECT s.road_segment_id FROM {RoadNetworkTopologyTables.RoadSegments} s WHERE s.road_segment_id = ANY(?)");
            parameters.Add(ids.RoadSegmentIds.Select(x => x.ToInt32()).ToArray());
        }

        if (ids?.RoadNodeIds.Count > 0)
        {
            var roadNodeIds = ids.RoadNodeIds.Select(x => x.ToInt32()).ToArray();
            seeds.Add($"SELECT s.road_segment_id FROM {RoadNetworkTopologyTables.RoadSegments} s WHERE s.start_node_id = ANY(?) OR s.end_node_id = ANY(?)");
            parameters.Add(roadNodeIds);
            parameters.Add(roadNodeIds);
        }

        if (ids?.GradeSeparatedJunctionIds.Count > 0)
        {
            var gradeSeparatedJunctionIds = ids.GradeSeparatedJunctionIds.Select(x => x.ToInt32()).ToArray();
            seeds.Add($"SELECT j.lower_road_segment_id FROM {RoadNetworkTopologyTables.GradeSeparatedJunctions} j WHERE j.grade_separated_junction_id = ANY(?)");
            seeds.Add($"SELECT j.upper_road_segment_id FROM {RoadNetworkTopologyTables.GradeSeparatedJunctions} j WHERE j.grade_separated_junction_id = ANY(?)");
            parameters.Add(gradeSeparatedJunctionIds);
            parameters.Add(gradeSeparatedJunctionIds);
        }

        if (ids?.GradeJunctionIds.Count > 0)
        {
            var gradeJunctionIds = ids.GradeJunctionIds.Select(x => x.ToInt32()).ToArray();
            seeds.Add($"SELECT j.road_segment_id_1 FROM {RoadNetworkTopologyTables.GradeJunctions} j WHERE j.grade_junction_id = ANY(?)");
            seeds.Add($"SELECT j.road_segment_id_2 FROM {RoadNetworkTopologyTables.GradeJunctions} j WHERE j.grade_junction_id = ANY(?)");
            parameters.Add(gradeJunctionIds);
            parameters.Add(gradeJunctionIds);
        }

        // Taken from the road segments themselves, so a junction pointing at a road segment that has no document
        // brings nothing into scope.
        return new RoadSegmentScope($@"
SELECT rs.road_segment_id AS id
FROM {RoadNetworkTopologyTables.RoadSegments} rs
WHERE rs.road_segment_id IN ({string.Join(" UNION ", seeds)})", parameters.ToArray());
    }

    public async Task<ScopedRoadNetwork> LoadWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        if (roadSegmentIds.Count == 0)
        {
            return new ScopedRoadNetwork(roadNetworkId);
        }

        var scope = new RoadSegmentScope($@"
WITH requested AS (
    SELECT rs.road_segment_id, rs.start_node_id, rs.end_node_id
    FROM {RoadNetworkTopologyTables.RoadSegments} rs
    WHERE rs.has_migrated AND rs.road_segment_id = ANY(?)
)
-- The road segments the caller named, knotted into the road network or not. An ingeschetst road segment has no
-- road nodes, and the join below compares them, so it answers for no road segment without them - not even with
-- itself, NULL being equal to nothing in SQL. Asked for on its own it used to load an empty road network, and
-- the action then had nothing to work on and said nothing about it.
SELECT requested.road_segment_id AS id FROM requested
UNION
-- And every road segment meeting one of them at a road node: what an action does to the one is a statement
-- about the other.
SELECT other.road_segment_id AS id
FROM requested
JOIN {RoadNetworkTopologyTables.RoadSegments} other ON other.has_migrated AND (
    requested.start_node_id = other.start_node_id
    OR requested.start_node_id = other.end_node_id
    OR requested.end_node_id = other.start_node_id
    OR requested.end_node_id = other.end_node_id
)", [roadSegmentIds.Select(x => x.ToInt32()).ToArray()]);

        return await Load(session, roadNetworkId, scope, namedRoadNodeIds: [], migratedGradeSeparatedJunctionsOnly: true);
    }

    // One batch: the road segments in scope, the road nodes they start and end at plus the ones named, and the
    // junctions lying on them. Each query works out the scope for itself; Postgres answers it from the indexes on the
    // generated columns.
    private static async Task<ScopedRoadNetwork> Load(IDocumentSession session, ScopedRoadNetworkId roadNetworkId, RoadSegmentScope scope, int[] namedRoadNodeIds, bool migratedGradeSeparatedJunctionsOnly)
    {
        var roadNodesSql = $@"
road_node_id IN (
  WITH scope AS ({scope.Sql})
  SELECT rs.start_node_id FROM {RoadNetworkTopologyTables.RoadSegments} rs JOIN scope ON scope.id = rs.road_segment_id
  UNION
  SELECT rs.end_node_id FROM {RoadNetworkTopologyTables.RoadSegments} rs JOIN scope ON scope.id = rs.road_segment_id
)
OR road_node_id = ANY(?)";
        object[] roadNodesParameters = [..scope.Parameters, namedRoadNodeIds];
        var roadSegmentsSql = $"road_segment_id IN ({scope.Sql})";
        var gradeSeparatedJunctionsSql = $@"
grade_separated_junction_id IN (
  WITH scope AS ({scope.Sql})
  SELECT j.grade_separated_junction_id
  FROM {RoadNetworkTopologyTables.GradeSeparatedJunctions} j
  JOIN scope ON scope.id = j.lower_road_segment_id OR scope.id = j.upper_road_segment_id
  {(migratedGradeSeparatedJunctionsOnly ? "WHERE j.has_migrated" : string.Empty)}
)";
        var gradeJunctionsSql = $@"
grade_junction_id IN (
  WITH scope AS ({scope.Sql})
  SELECT j.grade_junction_id
  FROM {RoadNetworkTopologyTables.GradeJunctions} j
  JOIN scope ON scope.id = j.road_segment_id_1 OR scope.id = j.road_segment_id_2
)";
        var scopeParameters = scope.Parameters;

        var batch = session.CreateBatchQuery();
        var roadNodes = batch.Query<RoadNode>().Where(x => x.MatchesSql(roadNodesSql, roadNodesParameters)).ToList();
        var roadSegments = batch.Query<RoadSegment>().Where(x => x.MatchesSql(roadSegmentsSql, scopeParameters)).ToList();
        var gradeSeparatedJunctions = batch.Query<GradeSeparatedJunction>().Where(x => x.MatchesSql(gradeSeparatedJunctionsSql, scopeParameters)).ToList();
        var gradeJunctions = batch.Query<GradeJunction>().Where(x => x.MatchesSql(gradeJunctionsSql, scopeParameters)).ToList();

        await batch.Execute();

        return new ScopedRoadNetwork(roadNetworkId, await roadNodes, await roadSegments, await gradeSeparatedJunctions, await gradeJunctions);
    }

    // The extract documents are kept apart from the aggregates, by a projection of their own, but under the same id
    // as the entity they are about - so the scope is worked out on the aggregate documents and the extract documents
    // are taken by their id, all in one batch.
    public async Task<RoadNetworkExtractDocuments<TRoadNode, TRoadSegment, TGradeSeparatedJunction>> LoadForExtract<TRoadNode, TRoadSegment, TGradeSeparatedJunction>(IDocumentSession session, Geometry geometry)
        where TRoadNode : class
        where TRoadSegment : class
        where TGradeSeparatedJunction : class
    {
        if (geometry is null || geometry.IsEmpty)
        {
            throw new ArgumentException("Geometry must be specified.");
        }

        var scope = new RoadSegmentScope($@"
SELECT rs.road_segment_id AS id
FROM {RoadNetworkTopologyTables.RoadSegments} rs
WHERE ST_Intersects(rs.geometry, ST_SetSRID(ST_GeomFromText(?), ?))
  AND NOT rs.is_removed
  AND (NOT rs.has_migrated OR rs.status = ANY(?))", [geometry.AsText(), geometry.SRID, ExtractRoadSegmentStatuses]);

        var roadNodesSql = $@"
id IN (
  WITH scope AS ({scope.Sql})
  SELECT n.road_node_id
  FROM {RoadNetworkTopologyTables.RoadSegments} rs
  JOIN scope ON scope.id = rs.road_segment_id
  JOIN {RoadNetworkTopologyTables.RoadNodes} n ON n.road_node_id = rs.start_node_id OR n.road_node_id = rs.end_node_id
  WHERE NOT n.is_removed
)";
        var roadSegmentsSql = $"id IN ({scope.Sql})";
        var gradeSeparatedJunctionsSql = $@"
id IN (
  WITH scope AS ({scope.Sql})
  SELECT j.grade_separated_junction_id
  FROM {RoadNetworkTopologyTables.GradeSeparatedJunctions} j
  JOIN scope ON scope.id = j.lower_road_segment_id OR scope.id = j.upper_road_segment_id
  WHERE NOT j.is_removed
)";
        var scopeParameters = scope.Parameters;

        var batch = session.CreateBatchQuery();
        var roadNodes = batch.Query<TRoadNode>().Where(x => x.MatchesSql(roadNodesSql, scopeParameters)).ToList();
        var roadSegments = batch.Query<TRoadSegment>().Where(x => x.MatchesSql(roadSegmentsSql, scopeParameters)).ToList();
        var gradeSeparatedJunctions = batch.Query<TGradeSeparatedJunction>().Where(x => x.MatchesSql(gradeSeparatedJunctionsSql, scopeParameters)).ToList();

        await batch.Execute();

        return new RoadNetworkExtractDocuments<TRoadNode, TRoadSegment, TGradeSeparatedJunction>(await roadNodes, await roadSegments, await gradeSeparatedJunctions);
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

    // A subquery selecting the ids of the road segments in scope as "id", and the parameters its placeholders take.
    private sealed record RoadSegmentScope(string Sql, object[] Parameters);
}


public sealed record IdempotentSession(string Id);
