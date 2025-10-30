namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;

public static class RoadNetworkChangesProjectionExtensions
{
    public static StoreOptions AddRoadNetworkChangesProjection(this StoreOptions options,
        string changesDocumentAlias,
        IRoadNetworkChangesProjection[] projections,
        ProjectionLifecycle lifecycle = ProjectionLifecycle.Async)
    {
        options.Projections.Add(new RoadNetworkChangesProjection(projections), lifecycle,
            asyncConfiguration: opts => { opts.BatchSize = 100; });

        options.Schema.For<RoadNetworkChangeProjectionItem>()
            .DocumentAlias(changesDocumentAlias)
            .Identity(x => x.Id);

        return options;
    }
}

public class RoadNetworkChangesProjection : IProjection
{
    private readonly IRoadNetworkChangesProjection[] _projections;

    internal RoadNetworkChangesProjection(IRoadNetworkChangesProjection[] projections)
    {
        _projections = projections;
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

            var roadNetworkChangeDocument = await session.LoadAsync<RoadNetworkChangeProjectionItem>(causationId, cancellation);
            if (roadNetworkChangeDocument is not null)
            {
                continue;
            }

            session.Store(new RoadNetworkChangeProjectionItem
            {
                Id = causationId
            });

            var processEvents = roadNetworkChangeEvents;

            var isLastBatch = i == roadNetworkChanges.Length - 1;
            if (isLastBatch)
            {
                processEvents = operations.Events.QueryAllRawEvents()
                    .Where(x => x.CausationId == causationId) //TODO-pr add index on causationId
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

public sealed class RoadNetworkChangeProjectionItem
{
    public string Id { get; set; }
}
