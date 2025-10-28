namespace RoadRegistry.Infrastructure.MartenDb.Store;

using BackOffice;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Dapper;
using Marten;
using Marten.Schema;
using NetTopologySuite.Geometries;
using Projections;
using RoadNetwork;
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

    private record RoadNetworkSegment(int Id, int StartNodeId, int EndNodeId);

    public async Task<RoadNetwork> Load(
        RoadNetworkChanges changes,
        CancellationToken cancellationToken)
    {
        //TODO-pr load using envelope, gekende ids, en 1 niveau verder, bvb wegsegment x -> knope en dan die zijn wegsegment (dit is relevant voor bulk delete roadsegments)
        await using var session = _store.LightweightSession();

        var boundingBox = new Polygon(new LinearRing([
            new Coordinate(changes.Scope.MinX, changes.Scope.MinY),
            new Coordinate(changes.Scope.MinX, changes.Scope.MaxY),
            new Coordinate(changes.Scope.MaxX, changes.Scope.MaxY),
            new Coordinate(changes.Scope.MaxX, changes.Scope.MinY),
            new Coordinate(changes.Scope.MinX, changes.Scope.MinY)
        ]));

        var sql = @$"
SELECT id as Id, start_node_id as StartNodeId, end_node_id as EndNodeId
FROM {RoadNetworkTopologyProjection.TableName}
WHERE ST_Intersects(geometry, ST_GeomFromText(@wkt, {WellKnownGeometryFactories.Default.SRID}))";
        var segments = (await session.Connection.QueryAsync<RoadNetworkSegment>(sql, new { wkt = boundingBox.AsText() }))
            .ToList();

        var network = await Load(
            session,
            segments.SelectMany(x => new[] { x.StartNodeId, x.EndNodeId }).Distinct().Select(x => new RoadNodeId(x)).ToList(),
            segments.Select(x => new RoadSegmentId(x.Id)).ToList());
        return network;
    }

    private async Task<RoadNetwork> Load(
        IDocumentSession session,
        ICollection<RoadNodeId> roadNodeIds,
        ICollection<RoadSegmentId> roadSegmentIds)
    {
        if (!roadSegmentIds.Any())
        {
            return RoadNetwork.Empty;
        }

        //TODO-pr convert identifier to streamkey (=AggregrateId)
        var roadSegmentSnapshots = await session.LoadManyAsync<RoadSegment>(roadSegmentIds.Select(x => $"RoadSegment-{x}"));
        var roadNodeSnapshots = await session.LoadManyAsync<RoadNode>(roadNodeIds.Select(x => x.ToInt32().ToString()));
        //TODO-pr gradeseparatedjunctions

        if (roadSegmentSnapshots.Any())
        {
            return new RoadNetwork(roadNodeSnapshots, roadSegmentSnapshots, []);
        }

        // rebuild in progress? load data from aggregate streams
        var roadNodes = new List<RoadNode>();
        var roadSegments = new List<RoadSegment>();
        //TODO-pr gradeseparatedjunctions

        //TODO-pr build stream names
        // foreach (var id in roadNodeIds)
        // {
        //     roadNodes.Add(await session.Events.AggregateStreamAsync<RoadNode>(id));
        // }
        // foreach (var id in roadSegmentIds)
        // {
        //     roadSegments.Add(await session.Events.AggregateStreamAsync<RoadSegment>(id));
        // }

        return new RoadNetwork(roadNodes, roadSegments, []);
    }

    public async Task Save(RoadNetwork roadNetwork, CancellationToken cancellationToken)
    {
        await using var session = _store.LightweightSession();
        session.CausationId = Guid.NewGuid().ToString();

        SaveEntities(roadNetwork.RoadSegments, session);
        SaveEntities(roadNetwork.RoadNodes, session);
        SaveEntities(roadNetwork.GradeSeparatedJunctions, session);

        await session.SaveChangesAsync(cancellationToken);
    }

    private static void SaveEntities<TKey, TEntity>(IReadOnlyDictionary<TKey, TEntity> entities, IDocumentSession session)
        where TEntity : IMartenAggregateRootEntity
    {
        foreach(var entity in entities.Where(x => x.Value.HasChanges()))
        {
            foreach (var @event in entity.Value.GetChanges())
            {
                if (@event is ICreatedEvent)
                {
                    session.Events.StartStream(entity.Value.Id, @event);
                }
                else
                {
                    session.Events.Append(entity.Value.Id, @event);
                }
            }

            session.Store(entity.Value);
        }
    }
}
