namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using BackOffice;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;

public abstract class RoadNetworkChangesProjection : IProjection
{
    public int BatchSize { get; }
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;
    private readonly ILogger _logger;
    private readonly string _projectionName;
    protected const int DefaultBatchSize = 5000;
    private bool? _isCatchingUp;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections, ILoggerFactory loggerFactory, int batchSize = DefaultBatchSize)
    {
        _projections = projections;
        BatchSize = batchSize;
        _logger = loggerFactory.CreateLogger(GetType());
        _projectionName = GetType().Name;
    }

    public void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNetworkChangesProjectionProgression>()
            .DatabaseSchemaName(WellKnownSchemas.MartenEventStore)
            .DocumentAlias("roadnetworkchangesprojection_progression")
            .Identity(x => x.Id)
            .Duplicate(x => x.ProjectionName, configure: index => { index.Name = "ix_changesprojection_projectionname"; }, notNull: true);

        ConfigureSchema(options);
    }

    protected virtual void ConfigureSchema(StoreOptions options)
    {
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        try
        {
            var batchCorrelationIds = events.Select(x => x.CorrelationId!).Distinct().ToList();

            var processedProjectionProgressions = await operations.Query<RoadNetworkChangesProjectionProgression>()
                .Where(x => x.ProjectionName == _projectionName && batchCorrelationIds.Contains(x.Id))
                .ToListAsync(cancellation);

            var processOnlyCurrentBatch = DetermineToProcessOnlyCurrentBatch(events);
            if (processOnlyCurrentBatch)
            {
                await ProcessEvents(operations, events, processedProjectionProgressions, cancellation);
            }
            else
            {
                var queriedEvents = operations.Events.QueryAllRawEvents()
                    .Where(x => batchCorrelationIds.Contains(x.CorrelationId!)) //TODO-pr add index on correlationId
                    .ToList();
                await ProcessEvents(operations, queriedEvents, processedProjectionProgressions, cancellation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error trying to project events from {events.First().Sequence} to {events.Last().Sequence}");
            throw;
        }
    }

    private bool DetermineToProcessOnlyCurrentBatch(IReadOnlyList<IEvent> events)
    {
        if (_isCatchingUp is null)
        {
            _isCatchingUp = events.Count == BatchSize;
        }
        else if (events.Count < BatchSize)
        {
            _isCatchingUp = false;
        }

        return _isCatchingUp!.Value || events.Count < BatchSize;
    }

    private async Task ProcessEvents(IDocumentOperations operations, IReadOnlyList<IEvent> events, IReadOnlyList<RoadNetworkChangesProjectionProgression> processedProjectionProgressions, CancellationToken cancellation)
    {
        var eventsPerCorrelationId = events
            .Where(x => x.CorrelationId is not null && (x.StreamKey is null || !x.StreamKey.StartsWith("mt_")))
            .GroupBy(x => x.CorrelationId!)
            .OrderBy(x => x.First().Sequence)
            .ToList();

        foreach (var eventsGrouping in eventsPerCorrelationId.Select((g, i) => (CorrelationId: g.Key, Events: g.OrderBy(x => x.Sequence).ToList())))
        {
            var lastSequenceId = eventsGrouping.Events.Max(x => x.Sequence);
            var projectionProgression = processedProjectionProgressions.SingleOrDefault(x => x.Id == eventsGrouping.CorrelationId);

            var eventsToProcess = projectionProgression is not null
                ? eventsGrouping.Events.Where(x => x.Sequence > projectionProgression.LastSequenceId).ToList()
                : eventsGrouping.Events;
            if (eventsToProcess.Any())
            {
                foreach (var projection in _projections)
                {
                    await projection.Project(operations, eventsToProcess, cancellation).ConfigureAwait(false);
                }
            }

            if (projectionProgression is null)
            {
                operations.Insert(new RoadNetworkChangesProjectionProgression
                {
                    Id = eventsGrouping.CorrelationId,
                    ProjectionName = _projectionName,
                    LastSequenceId = lastSequenceId
                });
            }
            else if (lastSequenceId > projectionProgression.LastSequenceId)
            {
                projectionProgression.LastSequenceId = lastSequenceId;
            }
        }
    }
}

public sealed class RoadNetworkChangesProjectionProgression
{
    public required string Id { get; set; }
    public required string ProjectionName { get; set; }
    public required long LastSequenceId { get; set; }
}
