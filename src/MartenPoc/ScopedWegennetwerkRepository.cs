namespace MartenPoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JasperFx.Events.Projections;
    using Marten;
    using Marten.Events.Projections;
    using Marten.Services;
    using Microsoft.CodeAnalysis.Diagnostics;

    public class ScopedWegennetwerkRepository
    {
        private readonly IDocumentStore _store;

        public ScopedWegennetwerkRepository(IDocumentStore store)
        {
            _store = store;
        }

        public static void Configure(StoreOptions options)
        {
            options.Connection("Host=localhost;port=5440;Username=postgres;Password=postgres");
            //options.DatabaseSchemaName = "road";
            //options.UseSystemTextJsonForSerialization();

            options.Events.MetadataConfig.CausationIdEnabled = true;

            options.Projections.Add(new RoadNetworkProjection(), ProjectionLifecycle.Inline);
            options.Projections.Snapshot<RoadNetworkSegment>(SnapshotLifecycle.Inline);

        }

        public async Task<ScopedWegennetwerk> Load(
            IEnumerable<Guid> wegknoopIds,
            IEnumerable<Guid> wegsegmentIds)
        {
            await using var session = _store.LightweightSession();

            var wegknoopTasks = wegknoopIds
                .Select(id => session.Events.AggregateStreamAsync<Wegknoop>(id))
                .ToList();

            var wegsegmentTasks = wegsegmentIds
                .Select(id => session.Events.AggregateStreamAsync<Wegsegment>(id))
                .ToList();

            var wegknopen = await Task.WhenAll(wegknoopTasks);
            var wegsegmenten = await Task.WhenAll(wegsegmentTasks);

            return new ScopedWegennetwerk(wegknopen!, wegsegmenten!);
        }

        public async Task SaveRoadNetworkChange(ScopedWegennetwerk netwerk)
        {
            await using var session = _store.LightweightSession();
            session.CausationId = Guid.NewGuid().ToString();

            foreach (var aggregateEvents in netwerk.UncommittedEvents)
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

            await session.SaveChangesAsync();

            netwerk.UncommittedEvents.Clear();
        }
    }
}
