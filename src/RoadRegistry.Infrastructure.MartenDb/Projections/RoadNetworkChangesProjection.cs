namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Diagnostics;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

public sealed record RoadNetworkChangesProjectionProgression(string Id, string ProjectionName, long LastSequenceId);

public abstract class RoadNetworkChangesProjection : IProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;
    }

    public void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNetworkChangesProjectionProgression>()
            .DatabaseSchemaName("eventstore")
            .DocumentAlias("roadnetworkchangesprojection_progression")
            .Identity(x => x.Id)
            .Index(x => x.ProjectionName, i =>
            {
                i.Name = "ix_changesprojection_projectionname";
            });

        ConfigureSchema(options);
    }

    protected virtual void ConfigureSchema(StoreOptions options)
    {
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        var projectionName = GetType().Name;
        var correlationIds = events.Select(x => x.CorrelationId!).Distinct().ToList();

        var processedCorrelationIds = await operations.Query<RoadNetworkChangesProjectionProgression>()
            .Where(x => x.ProjectionName == projectionName && correlationIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellation);
        var unprocessedCorrelationIds = correlationIds.Except(processedCorrelationIds).ToList();

        var eventsPerCorrelationId = operations.Events.QueryAllRawEvents()
            .Where(x => unprocessedCorrelationIds.Contains(x.CorrelationId!)) //TODO-pr add index on correlationId
            .ToList()
            .GroupBy(x => x.CorrelationId!)
            .ToList();

        foreach (var eventsGrouping in eventsPerCorrelationId.Select((g, i) => (CorrelationId: g.Key, Events: g.ToArray())))
        {
            foreach (var projection in _projections)
            {
                await projection.Project(operations, eventsGrouping.Events, cancellation).ConfigureAwait(false);
            }

            operations.Insert(new RoadNetworkChangesProjectionProgression(eventsGrouping.CorrelationId, projectionName, eventsGrouping.Events.Max(x => x.Sequence)));
        }
    }
}
