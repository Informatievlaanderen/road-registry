namespace RoadRegistry.Infrastructure.MartenDb.Projections;

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
    private bool IsCatchingUp => _isCatchingUp ?? false;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections, ILoggerFactory loggerFactory, int batchSize = DefaultBatchSize)
    {
        _projections = projections;
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
                return (CorrelationId: g.Key, ProgressionId: progressionId, LastSeq: lastSeq, Progression: progression, ToProcess: toProcess);
            })
            .Where(x => x.ToProcess.Count > 0)
            .ToList();

        foreach (var work in correlationWork)
        {
            foreach (var evt in work.ToProcess)
            {
                _logger.LogInformation("Processing event {Sequence}: {EventTypeName}", evt.Sequence, evt.EventTypeName);

                foreach (var projection in _projections)
                {
                    if (projection is RoadNetworkChangesConnectedProjection roadNetworkChangesConnectedProjection)
                    {
                        roadNetworkChangesConnectedProjection.IsCatchingUp = IsCatchingUp;
                    }

                    await projection.Project(operations, [evt], cancellation).ConfigureAwait(false);
                }
            }

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
