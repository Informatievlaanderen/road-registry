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
    protected bool IsCatchingUp => _isCatchingUp ?? false;

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
            var batchProgressionIds = batchCorrelationIds.Select(BuildProgressionId).ToList();

            var processedProjectionProgressions = await operations.Query<RoadNetworkChangesProjectionProgression>()
                .Where(x => x.ProjectionName == _projectionName && batchProgressionIds.Contains(x.Id))
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

        // Pre-compute eventsToProcess for every correlation upfront so the projection loop below uses
        // a stable snapshot rather than re-evaluating the Where filter inside the nested loop.
var progressionById = processedProjectionProgressions.ToDictionary(x => x.Id);

var correlationWork = eventsPerCorrelationId
    .Select(g =>
    {
        var orderedEvents = g.OrderBy(x => x.Sequence).ToList();
        var progressionId = BuildProgressionId(g.Key);
        var lastSeq = orderedEvents[^1].Sequence;
        progressionById.TryGetValue(progressionId, out var progression);
        IReadOnlyList<IEvent> toProcess = progression is not null
            ? orderedEvents.Where(x => x.Sequence > progression.LastSequenceId).ToList()
            : orderedEvents;
        return (CorrelationId: g.Key, ProgressionId: progressionId, LastSeq: lastSeq, Progression: progression, ToProcess: toProcess);
    })
            .Where(x => x.ToProcess.Count > 0)
            .ToList();

// OUTER loop over sub-projections, INNER loop over correlations.
// This ensures all road-segment handlers run for every correlation (populating the Marten identity map)
// before any junction handlers run, preventing junction projections from referencing segments that
// haven't been projected yet.
        foreach (var projection in _projections)
        {
            if (projection is RoadNetworkChangesConnectedProjection connected)
            {
                connected.IsCatchingUp = IsCatchingUp;
            }

            foreach (var work in correlationWork)
            {
                await projection.Project(operations, work.ToProcess, cancellation).ConfigureAwait(false);
            }
        }

        // Update progressions after all projections have run for all correlations.
        foreach (var work in correlationWork)
        {
            if (work.Progression is null)
            {
                operations.Insert(new RoadNetworkChangesProjectionProgression
                {
                    Id = work.ProgressionId,
                    ProjectionName = _projectionName,
                    LastSequenceId = work.LastSeq
                });
            }
            else if (work.LastSeq > work.Progression.LastSequenceId)
            {
                work.Progression.LastSequenceId = work.LastSeq;
                operations.Store(work.Progression);
            }
        }
    }

    private string BuildProgressionId(string correlationId)
    {
        return $"{_projectionName}-{correlationId}";
    }
}

public sealed class RoadNetworkChangesProjectionProgression
{
    public required string Id { get; set; }
    public required string ProjectionName { get; set; }
    public required long LastSequenceId { get; set; }
}
