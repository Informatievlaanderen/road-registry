namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;

// The Marten IProjection that Marten's async daemon drives. It groups a batch by correlation id, skips already-processed
// correlations (via the RoadNetworkChangesProjectionProgression document), and hands the work to DispatchAsync. How the
// events are actually applied - and where the read-side projection state lives - is decided by the concrete driver
// (MartenBackedRoadNetworkChangesProjection or DbContextBackedRoadNetworkChangesProjection<TDbContext>).
public abstract class RoadNetworkChangesProjection : IProjection
{
    public int BatchSize { get; }
    private readonly ILogger _logger;
    private readonly string _projectionName;
    protected const int DefaultBatchSize = 5000;
    private bool? _isCatchingUp;

    protected bool IsCatchingUp => _isCatchingUp ?? false;
    protected string ProjectionName => _projectionName;
    protected ILogger Logger => _logger;

    protected RoadNetworkChangesProjection(ILoggerFactory loggerFactory, int batchSize = DefaultBatchSize)
    {
        BatchSize = batchSize;
        _logger = loggerFactory.CreateLogger(GetType());
        _projectionName = GetType().Name;
    }

    public void Configure(StoreOptions options)
    {
        options.ConfigureRoadNetworkChangesProgression();

        ConfigureSchema(options);
    }

    protected virtual void ConfigureSchema(StoreOptions options)
    {
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        try
        {
            await UpdateCatchingUpState(operations, events, cancellation);

            var batchCorrelationIds = events.Select(x => x.CorrelationId!).Distinct().ToList();
            var batchProgressionIds = batchCorrelationIds.Select(BuildProgressionId).ToList();

            var processedProjectionProgressions = await operations.Query<RoadNetworkChangesProjectionProgression>()
                .Where(x => x.ProjectionName == _projectionName && batchProgressionIds.Contains(x.Id))
                .ToListAsync(cancellation);

            var pageMaxSequence = events.Max(x => x.Sequence);
            var tailEvents = IsCatchingUp
                ? []
                : await operations.Events.QueryAllRawEvents()
                    .Where(x => batchCorrelationIds.Contains(x.CorrelationId!) && x.Sequence > pageMaxSequence)
                    .ToListAsync(cancellation);

            var allEvents = tailEvents.Count > 0 ? events.Concat(tailEvents).ToList() : events;

            await ProcessEvents(operations, allEvents, processedProjectionProgressions, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error trying to project events from {events.First().Sequence} to {events.Last().Sequence}");
            throw;
        }
    }

    private async Task UpdateCatchingUpState(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        if (_isCatchingUp is null)
        {
            var startupHighWaterMark = await operations.GetHighWaterMark(cancellation);
            _isCatchingUp = events.Max(x => x.Sequence) <= startupHighWaterMark;
        }
        else if (events.Count < BatchSize)
        {
            _isCatchingUp = false;
        }
    }

    private async Task ProcessEvents(IDocumentOperations operations, IReadOnlyList<IEvent> events, IReadOnlyList<RoadNetworkChangesProjectionProgression> processedProjectionProgressions, CancellationToken cancellation)
    {
        var eventsPerCorrelationId = events
            .Where(x => x.CorrelationId is not null && (x.StreamKey is null || !x.StreamKey.StartsWith("mt_")))
            .GroupBy(x => x.CorrelationId!)
            .OrderBy(x => x.First().Sequence)
            .ToList();

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
                return new CorrelationWorkItem(g.Key, progressionId, lastSeq, progression, toProcess);
            })
            .Where(x => x.ToProcess.Count > 0)
            .ToList();

        await DispatchAsync(operations, correlationWork, cancellation).ConfigureAwait(false);

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

    // Applies the per-correlation work to the sub-projections. The concrete driver decides what "session" the
    // sub-projections write to (the Marten operations, or a freshly created TDbContext) and owns any read-side
    // projection-state/commit for that session.
    protected abstract Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, CancellationToken cancellationToken);

    private string BuildProgressionId(string correlationId)
    {
        return $"{_projectionName}-{correlationId}";
    }

    // One correlation's slice of a batch: the events still to process (after the Marten progression filter) plus the
    // progression bookkeeping the base advances once DispatchAsync has run.
    protected sealed record CorrelationWorkItem(
        string CorrelationId,
        string ProgressionId,
        long LastSeq,
        RoadNetworkChangesProjectionProgression? Progression,
        IReadOnlyList<IEvent> ToProcess);
}

public sealed class RoadNetworkChangesProjectionProgression
{
    public required string Id { get; set; }
    public required string ProjectionName { get; set; }
    public required long LastSequenceId { get; set; }
}
