namespace MartenPoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dapper;
    using JasperFx.Events.Projections;
    using Marten;
    using Marten.Events.Projections;
    using NetTopologySuite.Geometries;
    using Npgsql;

    public class ScopedWegennetwerkRepository
    {
        private readonly IDocumentStore _store;

        public ScopedWegennetwerkRepository(IDocumentStore store)
        {
            _store = store;
        }

        public static void Configure(StoreOptions options)
        {
            options.Connection(new NpgsqlDataSourceBuilder("Host=localhost;port=5440;Username=postgres;Password=postgres")
                //.UseNetTopologySuite()
                .Build());
            options.DatabaseSchemaName = "road";
            //options.UseSystemTextJsonForSerialization();

            options.Events.MetadataConfig.CausationIdEnabled = true;

            options.Projections.Add(new RoadNetworkTopologyProjection(), ProjectionLifecycle.Inline);

            options.Projections.Snapshot<Wegsegment>(SnapshotLifecycle.Inline);
            // options.Schema.For<Wegsegment>().Metadata(opts =>
            // {
            //     opts.Version.MapTo(x => x.Version);
            // });
            options.Projections.Snapshot<Wegknoop>(SnapshotLifecycle.Inline);
            // options.Schema.For<Wegknoop>().Metadata(opts =>
            // {
            //     opts.Version.MapTo(x => x.Version);
            // });
        }

        private record RoadNetworkSegment(Guid Id, Guid StartNodeId, Guid EndNodeId);

        public async Task<ScopedWegennetwerk> Load(Geometry boundingBox)
        {
            await using var session = _store.LightweightSession();

            var segments = (await session.Connection.QueryAsync<RoadNetworkSegment>(@"
SELECT id as Id, start_node_id as StartNodeId, end_node_id as EndNodeId
FROM road.roadnetworksegments
WHERE ST_Intersects(geometry, ST_GeomFromText(@wkt, 0))", new { wkt = boundingBox.AsText() }))
                .ToList();

            var network = await Load(
                session,
                segments.SelectMany(x => new[] { x.StartNodeId, x.EndNodeId }).Distinct().ToList(),
                segments.Select(x => x.Id).ToList());
            return network;
        }

        private async Task<ScopedWegennetwerk> Load(
            IDocumentSession session,
            ICollection<Guid> wegknoopIds,
            ICollection<Guid> wegsegmentIds)
        {
            if (!wegsegmentIds.Any())
            {
                return ScopedWegennetwerk.Empty;
            }

            var wegsegmentSnapshots = await session.LoadManyAsync<Wegsegment>(wegsegmentIds);
            var wegknoopSnapshots = await session.LoadManyAsync<Wegknoop>(wegknoopIds);

            if (wegsegmentSnapshots.Any())
            {
                return new ScopedWegennetwerk(wegknoopSnapshots, wegsegmentSnapshots);
            }

            // rebuild in progress? load data from aggregate streams
            var wegknopen = new List<Wegknoop>();
            var wegsegmenten = new List<Wegsegment>();

            foreach (var id in wegknoopIds)
            {
                wegknopen.Add(await session.Events.AggregateStreamAsync<Wegknoop>(id));
            }
            foreach (var id in wegsegmentIds)
            {
                wegsegmenten.Add(await session.Events.AggregateStreamAsync<Wegsegment>(id));
            }

            return new ScopedWegennetwerk(wegknopen, wegsegmenten);
        }

        public async Task SaveRoadNetworkChange(ScopedWegennetwerk network)
        {
            await using var session = _store.LightweightSession();
            session.CausationId = Guid.NewGuid().ToString();

            foreach (var aggregateEvents in network.UncommittedEvents)
            {
                foreach (var @event in aggregateEvents.Value)
                {
                    if (@event is ICreateEvent)
                    {
                        session.Events.StartStream(aggregateEvents.Key, @event);
                    }
                    else
                    {
                        session.Events.Append(aggregateEvents.Key, @event);
                    }
                }
            }

            SnapshotEntities(network, session);

            await session.SaveChangesAsync();

            network.UncommittedEvents.Clear();
        }

        private static void SnapshotEntities(ScopedWegennetwerk network, IDocumentSession session)
        {
            var changedSegments = network.Wegsegmenten.Where(x => network.UncommittedEvents.Keys.Contains(x.Id)).ToList();
            session.StoreObjects(changedSegments);

            var changedKnopen = network.Wegknopen.Where(x => network.UncommittedEvents.Keys.Contains(x.Id)).ToList();
            session.StoreObjects(changedKnopen);
        }
    }
}
