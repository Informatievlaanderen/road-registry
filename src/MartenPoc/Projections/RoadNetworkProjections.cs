namespace MartenPoc.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;

public class FlattenedRoadNetworkChangeProjection : IProjection
{
    private readonly IRoadNetworkChangeProjection[] _projections = [new RoadSegmentProjection(), new RoadNodeProjection()];

    public static void Configure(StoreOptions options)
    {
        options.Projections.Add(new FlattenedRoadNetworkChangeProjection(), ProjectionLifecycle.Async,
            //projectionName: "FlattenedRoadNetworkChangeProjection",
            asyncConfiguration: options => { options.BatchSize = 100; });

        options.Schema.For<RoadNetworkChange>()
            .DocumentAlias("projection_roadnetworkchanges")
            .Identity(x => x.Id);

        options.Schema.For<RoadSegment>()
            .DocumentAlias("projection_roadsegments")
            .Identity(x => x.Id);

        options.Schema.For<RoadNode>()
            .DocumentAlias("projection_roadnodes")
            .Identity(x => x.Id);
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        var roadNetworkChanges = events
            .GroupBy(x => x.CausationId!)
            .Select(x => x.ToArray())
            .ToArray();

        for (var i = 0; i < roadNetworkChanges.Length; i++)
        {
            var roadNetworkChangeEvents = roadNetworkChanges[i];
            var causationId = roadNetworkChangeEvents.First().CausationId!;

            await using var session = operations.DocumentStore.LightweightSession();

            var roadNetworkChangeDocument = await session.LoadAsync<RoadNetworkChange>(causationId, cancellation);
            if (roadNetworkChangeDocument is not null)
            {
                continue;
            }

            session.Store(new RoadNetworkChange
            {
                Id = causationId
            });

            var processEvents = roadNetworkChangeEvents;

            var isLastBatch = i == roadNetworkChanges.Length - 1;
            if (isLastBatch)
            {
                processEvents = operations.Events.QueryAllRawEvents()
                    .Where(x => x.CausationId == causationId) // note: add index on causationId
                    .ToArray();
            }

            foreach (var projection in _projections)
            {
                await projection.Project(processEvents, session);
            }

            await session.SaveChangesAsync(cancellation);
        }
    }
}

public sealed class RoadNetworkChange
{
    public string Id { get; set; }
}
