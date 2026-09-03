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
        using var projectionScope = _logger.BeginScope(new Dictionary<string, object> { ["ProjectionName"] = _projectionName });

        try
        {
            cancellation.ThrowIfCancellationRequested();

            await UpdateCatchingUpState(operations, events, cancellation);

            // Scope this to the events that can actually be projected, the same way ProcessEvents groups them. An event
            // without a correlation id cannot be grouped, and Marten's own bookkeeping streams are not ours to project;
            // carrying either into the queries below asks for progressions that cannot exist and tail-fetches events
            // for correlations that ProcessEvents then discards.
            var batchCorrelationIds = events.Where(IsProjectable).Select(x => x.CorrelationId!).Distinct().ToList();
            var batchProgressionIds = batchCorrelationIds.Select(BuildProgressionId).ToList();

            cancellation.ThrowIfCancellationRequested();
            var processedProjectionProgressions = batchCorrelationIds.Count > 0
                ? await operations.Query<RoadNetworkChangesProjectionProgression>()
                    .Where(x => x.ProjectionName == _projectionName && batchProgressionIds.Contains(x.Id))
                    .ToListAsync(cancellation)
                : [];

            var pageMaxSequence = events.Max(x => x.Sequence);
            cancellation.ThrowIfCancellationRequested();
            var tailEvents = IsCatchingUp || batchCorrelationIds.Count == 0
                ? []
                : await operations.Events.QueryAllRawEvents()
                    .Where(x => batchCorrelationIds.Contains(x.CorrelationId!) && x.Sequence > pageMaxSequence)
                    .ToListAsync(cancellation);

            var allEvents = tailEvents.Count > 0 ? events.Concat(tailEvents).ToList() : events;

            await ProcessEvents(operations, allEvents, processedProjectionProgressions, pageMaxSequence, cancellation);
        }
        catch (OperationCanceledException)
        {
            // A pending cancellation is the daemon stopping the shard, not a projection failure; nothing was
            // committed, so the batch replays whole on the next start.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error trying to project events from {events.First().Sequence} to {events.Last().Sequence}");
            throw;
        }
    }

    private async Task UpdateCatchingUpState(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        var wasCatchingUp = _isCatchingUp;
        var backlog = 0L;

        if (_isCatchingUp is null)
        {
            cancellation.ThrowIfCancellationRequested();
            var startupHighWaterMark = await operations.GetHighWaterMark(cancellation);
            var batchMaxSequence = events.Max(x => x.Sequence);

            _isCatchingUp = IsBehind(batchMaxSequence, startupHighWaterMark);
            backlog = startupHighWaterMark - batchMaxSequence;
        }
        else if (events.Count < BatchSize)
        {
            _isCatchingUp = false;
        }

        if (wasCatchingUp == _isCatchingUp)
        {
            return;
        }

        if (_isCatchingUp is true)
        {
            await OnCatchUpStartedAsync(backlog, cancellation).ConfigureAwait(false);
        }
        else
        {
            await OnCatchUpFinishedAsync(cancellation).ConfigureAwait(false);
        }
    }

    // Strictly less than: the high water mark is the store's ceiling, so a batch that reaches it has nothing beyond it
    // and sits at the tail, not behind it. Counting that as catching up would have an up-to-date projection run its
    // catch-up behaviour for a single event on every start - which, now that catching up takes the read model's indexes
    // apart, is the difference between a no-op and a rebuild of every index on the table.
    internal static bool IsBehind(long batchMaxSequence, long startupHighWaterMark)
    {
        return batchMaxSequence < startupHighWaterMark;
    }

    // Called once when the projection discovers it is behind, before the first batch is applied. A driver can use it to
    // trade read-model availability for write throughput while the read model is incomplete anyway.
    //
    // estimatedBacklog is how many events sit between this first batch and the high water mark. It is the difference
    // between "rebuilt from scratch" and "restarted after a deploy", which matters because the trade only pays off over
    // a long replay: a driver should not take a read model apart to catch up on a few thousand events.
    protected virtual Task OnCatchUpStartedAsync(long estimatedBacklog, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // Called once when the projection reaches the tail. It also fires on the first batch of a projection that was
    // already caught up at startup - which is deliberate: that is what lets a host killed mid-catch-up undo whatever
    // OnCatchUpStartedAsync had turned off, without needing to know that it crashed.
    protected virtual Task OnCatchUpFinishedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ProcessEvents(IDocumentOperations operations, IReadOnlyList<IEvent> events, IReadOnlyList<RoadNetworkChangesProjectionProgression> processedProjectionProgressions, long pageMaxSequence, CancellationToken cancellation)
    {
        // An event without a correlation id cannot be grouped and is dropped here. That is expected for Marten's own
        // bookkeeping streams, but for anything else it means the event is never applied and never will be, since the
        // progression moves on regardless - so say so rather than discarding it quietly.
        var discarded = events
            .Where(x => x.CorrelationId is null && !IsInternalStream(x))
            .ToList();
        if (discarded.Count > 0)
        {
            _logger.LogWarning("Discarding {Count} event(s) without a correlation id, they will not be projected: {Events}",
                discarded.Count,
                string.Join(", ", discarded.Select(x => $"{x.EventTypeName}@{x.Sequence}")));
        }

        var eventsPerCorrelationId = events
            .Where(IsProjectable)
            .GroupBy(x => x.CorrelationId!)
            .OrderBy(x => x.First().Sequence)
            .ToList();

        var progressionById = processedProjectionProgressions.ToDictionary(x => x.Id);

        var correlationWork = eventsPerCorrelationId
            .Select(g =>
            {
                cancellation.ThrowIfCancellationRequested();

                // Order by the emission ordinal stamped at save time (EventOrdinal header) so a correlation's
                // events replay in the order they were raised - Marten's seq_id does not preserve that order
                // (created events land last). Events without the header (pre-ordinal history) fall back to seq_id.
                var orderedEvents = g.OrderBy(GetChangeOrdinal).ThenBy(x => x.Sequence).ToList();
                var progressionId = BuildProgressionId(g.Key);
                // The watermark has to be the highest sequence seen for this correlation, not the last one in ordinal
                // order: the ordinal reorders the events (that is its whole purpose), so the last of the ordered list
                // can carry a lower sequence than its siblings. Recording that lower value leaves the correlation's
                // higher-sequence events above the watermark, and the next batch applies them a second time.
                var lastSeq = orderedEvents.Max(x => x.Sequence);
                progressionById.TryGetValue(progressionId, out var progression);
                IReadOnlyList<IEvent> toProcess = progression is not null
                    ? orderedEvents.Where(x => x.Sequence > progression.LastSequenceId).ToList()
                    : orderedEvents;
                return new CorrelationWorkItem(g.Key, progressionId, lastSeq, progression, toProcess);
            })
            .Where(x => x.ToProcess.Count > 0)
            .ToList();

        await DispatchAsync(operations, correlationWork, pageMaxSequence, cancellation).ConfigureAwait(false);

        foreach (var work in correlationWork)
        {
            cancellation.ThrowIfCancellationRequested();

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
    //
    // pageMaxSequence is the highest sequence the daemon delivered in this page - which is NOT the highest sequence in
    // correlationWork: the tail fetch above deliberately pulls a correlation's later events into this batch, so the
    // work can reach past the page. It is the only sequence a driver may record as "everything up to here is applied",
    // because it is the only one the next page is guaranteed to start after.
    protected abstract Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, long pageMaxSequence, CancellationToken cancellationToken);

    private string BuildProgressionId(string correlationId)
    {
        return $"{_projectionName}-{correlationId}";
    }

    // The single definition of "this projection can do something with that event", used both to scope the queries in
    // ApplyAsync and to group the work in ProcessEvents. Letting the two drift apart is what allows a tail-fetch to
    // pull events that are then thrown away.
    private static bool IsProjectable(IEvent @event)
    {
        return @event.CorrelationId is not null && !IsInternalStream(@event);
    }

    private static bool IsInternalStream(IEvent @event)
    {
        return @event.StreamKey is not null && @event.StreamKey.StartsWith("mt_");
    }

    // Reads the emission ordinal written as a Marten header at save time (EventOrdinal.HeaderKey). Events that
    // predate the ordinal (no header) sort last and keep their relative seq_id order via the secondary ThenBy.
    private static long GetChangeOrdinal(IEvent @event)
    {
        if (@event.Headers is not null
            && @event.Headers.TryGetValue(EventOrdinal.HeaderKey, out var value)
            && value is not null)
        {
            return Convert.ToInt64(value);
        }

        return long.MaxValue;
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
