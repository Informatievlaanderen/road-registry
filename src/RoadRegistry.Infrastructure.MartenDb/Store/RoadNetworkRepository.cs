namespace RoadRegistry.Infrastructure.MartenDb.Store;

using System.Data;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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
    public async Task<RoadNetwork> Load(
        IReadOnlyCollection<RoadNodeId> roadNodeIds,
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        IReadOnlyCollection<GradeSeparatedJunctionId> gradeSeparatedJunctionIds
    )
    {
        await using var session = Store.LightweightSession(IsolationLevel.Snapshot);

        var roadNetwork = await Load(
            session,
            roadNodeIds,
            roadSegmentIds,
            gradeSeparatedJunctionIds
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

    public async Task Save(RoadNetwork roadNetwork, string commandName, Provenance provenance, CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        AddChangesToSession(session, roadNetwork, commandName, provenance);

        await session.SaveChangesAsync(cancellationToken);
    }
    public void AddChangesToSession(IDocumentSession session, RoadNetwork roadNetwork, string commandName, Provenance provenance)
    {
        session.CorrelationId ??= Guid.NewGuid().ToString();
        session.CausationId = commandName;
        session.SetHeader("Provenance", provenance.ToDictionary());

        SaveEntities(roadNetwork.RoadSegments, session);
        SaveEntities(roadNetwork.RoadNodes, session);
        SaveEntities(roadNetwork.GradeSeparatedJunctions, session);
        foreach (var evt in roadNetwork.GetChanges())
        {
            session.Events.Append(roadNetwork.Id, evt);
        }
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

    private static void SaveEntities<TKey, TEntity>(IReadOnlyDictionary<TKey, TEntity> entities, IDocumentSession session)
        where TEntity : IMartenAggregateRootEntity
    {
        foreach (var entity in entities.Select(x => x.Value).Where(x => x.HasChanges()))
        {
            foreach (var @event in entity.GetChanges())
            {
                session.Events.AppendOrStartStream(entity.Id, @event);
            }

            session.Store(entity);
        }
    }

    private sealed record RoadNetworkTopologySegment(int RoadSegmentId, int StartNodeId, int EndNodeId, int? GradeSeparatedJunctionId);
}
