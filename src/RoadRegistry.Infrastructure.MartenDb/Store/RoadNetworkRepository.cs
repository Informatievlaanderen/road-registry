namespace RoadRegistry.Infrastructure.MartenDb.Store;

using Marten;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadNode;
using RoadSegment;

public class RoadNetworkRepository: IRoadNetworkRepository
{
    private readonly IDocumentStore _store;

    public RoadNetworkRepository(IDocumentStore store)
    {
        _store = store;
    }

    public async Task<RoadNetwork> Load(Geometry boundingBox, CancellationToken cancellationToken)
    {
//         await using var session = _store.LightweightSession();
//
//         var sql = @$"
// SELECT id as Id, start_node_id as StartNodeId, end_node_id as EndNodeId
// FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName}
// WHERE ST_Intersects(geometry, ST_GeomFromText(@wkt, 0))";
//         if (limitSegments is not null)
//         {
//             sql += $" LIMIT {limitSegments}";
//         }
//         var segments = (await session.Connection.QueryAsync<RoadNetworkSegment>(sql, new { wkt = boundingBox.AsText() }))
//             .ToList();
//
//         var network = await Load(
//             session,
//             segments.SelectMany(x => new[] { x.StartNodeId, x.EndNodeId }).Distinct().ToList(),
//             segments.Select(x => x.Id).ToList());
//         return network;
        throw new NotImplementedException();
    }

    private async Task<RoadNetwork> Load(
        IDocumentSession session,
        ICollection<Guid> wegknoopIds,
        ICollection<Guid> wegsegmentIds)
    {
        if (!wegsegmentIds.Any())
        {
            return RoadNetwork.Empty;
        }

        var roadSegmentSnapshots = await session.LoadManyAsync<RoadSegment>(wegsegmentIds);
        var roadNodeSnapshots = await session.LoadManyAsync<RoadNode>(wegknoopIds);
        //TODO-pr gradeseparatedjunctions

        if (roadSegmentSnapshots.Any())
        {
            return new RoadNetwork(roadNodeSnapshots, roadSegmentSnapshots, []);
        }

        // rebuild in progress? load data from aggregate streams
        var wegknopen = new List<RoadNode>();
        var wegsegmenten = new List<RoadSegment>();
        //TODO-pr gradeseparatedjunctions

        foreach (var id in wegknoopIds)
        {
            wegknopen.Add(await session.Events.AggregateStreamAsync<RoadNode>(id));
        }
        foreach (var id in wegsegmentIds)
        {
            wegsegmenten.Add(await session.Events.AggregateStreamAsync<RoadSegment>(id));
        }

        return new RoadNetwork(wegknopen, wegsegmenten, []);
    }

    public async Task Save(RoadNetwork roadNetwork, CancellationToken cancellationToken)
    {
        // await using var session = _store.LightweightSession();
        // session.CausationId = Guid.NewGuid().ToString();
        //
        // foreach (var aggregateEvents in network.UncommittedEvents)
        // {
        //     foreach (var @event in aggregateEvents.Value)
        //     {
        //         if (@event is ICreateEvent)
        //         {
        //             session.Events.StartStream(aggregateEvents.Key, @event);
        //         }
        //         else
        //         {
        //             session.Events.Append(aggregateEvents.Key, @event);
        //         }
        //     }
        // }
        //
        // SnapshotEntities(network, session);
        //
        // await session.SaveChangesAsync();
        //
        // network.UncommittedEvents.Clear();
        throw new NotImplementedException();
    }

    private static void SnapshotEntities(RoadNetwork network, IDocumentSession session)
    {
        // var changedSegments = network.Wegsegmenten.Where(x => network.UncommittedEvents.Keys.Contains(x.Id)).ToList();
        // session.StoreObjects(changedSegments);
        //
        // var changedKnopen = network.Wegknopen.Where(x => network.UncommittedEvents.Keys.Contains(x.Id)).ToList();
        // session.StoreObjects(changedKnopen);
    }
}
