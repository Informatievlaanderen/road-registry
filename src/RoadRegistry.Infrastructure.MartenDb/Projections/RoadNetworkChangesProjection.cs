namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;

public static class RoadNetworkChangesProjectionExtensions
{
    public static StoreOptions AddRoadNetworkChangesProjection(this StoreOptions options,
        string changesStateTableName,
        IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        options.Projections.Add(new RoadNetworkChangesProjection(projections),
            ProjectionLifecycle.Async,
            asyncConfiguration: opts => { opts.BatchSize = 1000; });

        options.Schema.For<RoadNetworkChangeProjectionItem>()
            .DocumentAlias(changesStateTableName)
            .Identity(x => x.Id);

        return options;
    }
}

internal class RoadNetworkChangesProjection : IProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    private const string ChangesPositionDocumentId = "roadnetworkchanges";

    internal RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        var roadNetworkChanges = events
            .GroupBy(x => x.CausationId!)
            .Select(x => x.ToList().AsReadOnly())
            .ToList();

        for (var i = 0; i < roadNetworkChanges.Count; i++)
        {
            var roadNetworkChangeEvents = roadNetworkChanges[i];
            var causationId = roadNetworkChangeEvents.First().CausationId!;

            await using var session = operations.DocumentStore.LightweightSession();

            var roadNetworkChangeDocument = await session.LoadAsync<RoadNetworkChangeProjectionItem>(ChangesPositionDocumentId, cancellation)
                ?? new RoadNetworkChangeProjectionItem
                {
                    Id = ChangesPositionDocumentId
                };
            if (roadNetworkChangeDocument.CurrentCausationId == causationId)
            {
                continue;
            }

            roadNetworkChangeDocument.CurrentCausationId = causationId;
            session.Store(roadNetworkChangeDocument);

            var processEvents = roadNetworkChangeEvents;

            var isLastBatch = i == roadNetworkChanges.Count - 1;
            if (isLastBatch)
            {
                processEvents = operations.Events.QueryAllRawEvents()
                    .Where(x => x.CausationId == causationId) //TODO-pr add index on causationId
                    .ToList()
                    .AsReadOnly();
            }

            foreach (var projection in _projections)
            {
                await projection.Project(processEvents, session, cancellation);
            }

            await session.SaveChangesAsync(cancellation);
        }
    }
}

public sealed class RoadNetworkChangeProjectionItem
{
    public string Id { get; set; }
    public string CurrentCausationId { get; set; }
}
