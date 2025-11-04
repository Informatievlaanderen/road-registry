namespace RoadRegistry.Infrastructure.MartenDb.Store;

using System.Data;
using BackOffice;
using Dapper;
using GradeSeparatedJunction;
using Marten;
using NetTopologySuite.Geometries;
using Projections;
using RoadNetwork;
using RoadNetwork.Events;
using RoadNode;
using RoadSegment;
using RoadSegment.ValueObjects;

public class RoadNetworkRepository: IRoadNetworkRepository
{
    private readonly IDocumentStore _store;

    public RoadNetworkRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry());
        var roadNetwork = await Load(
            session,
            roadNetworkChanges.RoadNodeIds.Union(ids.RoadNodeIds).ToList(),
            roadNetworkChanges.RoadSegmentIds.Union(ids.RoadSegmentIds).ToList(),
            roadNetworkChanges.GradeSeparatedJunctionIds.Union(ids.GradeSeparatedJunctionIds).ToList()
        );

        return roadNetwork;
    }

    public async Task Save(RoadNetwork roadNetwork, CancellationToken cancellationToken)
    {
        await using var session = _store.LightweightSession();

        var roadNetworkChangedEvent = roadNetwork.GetChanges().OfType<RoadNetworkChanged>().Single();
        session.CausationId = roadNetworkChangedEvent.CausationId;
        session.Events.Append(roadNetwork.Id, roadNetworkChangedEvent);

        SaveEntities(roadNetwork.RoadSegments, session);
        SaveEntities(roadNetwork.RoadNodes, session);
        SaveEntities(roadNetwork.GradeSeparatedJunctions, session);

        await session.SaveChangesAsync(cancellationToken);
    }

    private async Task<(IReadOnlyCollection<RoadNodeId> RoadNodeIds, IReadOnlyCollection<RoadSegmentId> RoadSegmentIds, IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds)> GetUnderlyingIds(
        IDocumentSession session, Geometry geometry)
    {
        if (geometry.IsEmpty)
        {
            return ([], [], []);
        }

        var sql = @$"
SELECT rs.id as RoadSegmentId, rs.start_node_id as StartNodeId, rs.end_node_id as EndNodeId, j.id as GradeSeparatedJunctionId
FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} rs
LEFT JOIN {RoadNetworkTopologyProjection.GradeSeparatedJunctionsTableName} j ON rs.id = j.upper_road_segment_id OR rs.id = j.lower_road_segment_id
WHERE ST_Intersects(rs.geometry, ST_GeomFromText(@wkt, {geometry.SRID}))";

        var segments = (await session.Connection.QueryAsync<RoadNetworkTopologySegment>(sql, new { wkt = geometry.AsText() })).ToList();

        return (
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

    private async Task<RoadNetwork> Load(
        IDocumentSession session,
        IReadOnlyCollection<RoadNodeId> roadNodeIds,
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        IReadOnlyCollection<GradeSeparatedJunctionId> gradeSeparatedJunctionIds
    )
    {
        if (!roadSegmentIds.Any())
        {
            return RoadNetwork.Empty;
        }

        var roadNodeSnapshots = await session.LoadRoadNodesAsync(roadNodeIds);
        var roadSegmentSnapshots = await session.LoadRoadSegmentsAsync(roadSegmentIds);
        var gradeSeparatedJunctionSnapshots = await session.LoadGradeSeparatedJunctionAsync(gradeSeparatedJunctionIds);

        if (roadSegmentSnapshots.Any())
        {
            return new RoadNetwork(roadNodeSnapshots, roadSegmentSnapshots, gradeSeparatedJunctionSnapshots);
        }

        //TODO-pr hoe bepalen of rebuild in progress is? is die vorige if-statement correct?
        //-> corrupte snapshots verwijderen + automatisch opslaan na verwerking, TODO die aggregates markeren dat ze dan altijd moeten worden opgeslagen

        var roadNodes = new List<RoadNode>();
        var roadSegments = new List<RoadSegment>();
        var gradeSeparatedJunction = new List<GradeSeparatedJunction>();

        foreach (var id in roadNodeIds)
        {
            roadNodes.Add((await session.Events.AggregateStreamAsync<RoadNode>(StreamKeyFactory.Create(typeof(RoadNode), id)))!);
        }
        foreach (var id in roadSegmentIds)
        {
            roadSegments.Add((await session.Events.AggregateStreamAsync<RoadSegment>(StreamKeyFactory.Create(typeof(RoadSegment), id)))!);
        }
        foreach (var id in gradeSeparatedJunctionIds)
        {
            gradeSeparatedJunction.Add((await session.Events.AggregateStreamAsync<GradeSeparatedJunction>(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), id)))!);
        }

        return new RoadNetwork(roadNodes, roadSegments, gradeSeparatedJunction);
    }

    private static void SaveEntities<TKey, TEntity>(IReadOnlyDictionary<TKey, TEntity> entities, IDocumentSession session)
        where TEntity : IMartenAggregateRootEntity
    {
        foreach(var entity in entities.Where(x => x.Value.HasChanges()))
        {
            foreach (var @event in entity.Value.GetChanges())
            {
                session.Events.AppendOrStartStream(entity.Value.Id, @event);
            }

            session.Store(entity.Value);
        }
    }

    private sealed record RoadNetworkTopologySegment(int RoadSegmentId, int StartNodeId, int EndNodeId, int? GradeSeparatedJunctionId);
}
