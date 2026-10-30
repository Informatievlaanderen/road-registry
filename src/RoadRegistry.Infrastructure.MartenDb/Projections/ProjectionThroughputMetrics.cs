namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Diagnostics;
using Microsoft.Extensions.Logging;

// Per-batch timings for a DbContext-backed projection, so the cost of a rebuild can be read off the log instead of
// inferred. The split matters: time in the handlers is read amplification and CPU, time in SaveChanges is EF's
// per-row command building and index maintenance, time in the bulk copy is raw insert throughput. Which of the three
// dominates decides what is worth optimising next.
internal sealed class ProjectionThroughputMetrics
{
    private readonly ILogger _logger;
    private readonly string _projectionName;
    private readonly int _logIntervalInBatches;
    private readonly TimeSpan _logInterval;
    private readonly Stopwatch _sinceStart = Stopwatch.StartNew();

    private long _batches;
    private long _events;
    private long _rowsBulkInserted;
    private long _rowsViaEntityFramework;
    private long _handlerMilliseconds;
    private long _bulkCopyMilliseconds;
    private long _saveChangesMilliseconds;

    // The same counters as they stood at the previous line. Every figure below is reported as the difference against
    // these rather than as a running total: a total tells you what the projection has done since it started, which is
    // the one thing you cannot compare between two runs, or between the stretch before a setting changed and the
    // stretch after it.
    private long _batchesAtLastLog;
    private long _eventsAtLastLog;
    private long _rowsBulkInsertedAtLastLog;
    private long _rowsViaEntityFrameworkAtLastLog;
    private long _handlerMillisecondsAtLastLog;
    private long _bulkCopyMillisecondsAtLastLog;
    private long _saveChangesMillisecondsAtLastLog;
    private TimeSpan _elapsedAtLastLog = TimeSpan.Zero;

    public ProjectionThroughputMetrics(ILogger logger, string projectionName, int logIntervalInBatches, int logIntervalInMinutes)
    {
        _logger = logger;
        _projectionName = projectionName;
        _logIntervalInBatches = Math.Max(1, logIntervalInBatches);
        _logInterval = TimeSpan.FromMinutes(Math.Max(1, logIntervalInMinutes));
    }

    public void RecordBatch(
        int events,
        int rowsBulkInserted,
        int rowsViaEntityFramework,
        TimeSpan handlerDuration,
        TimeSpan bulkCopyDuration,
        TimeSpan saveChangesDuration,
        bool isCatchingUp)
    {
        _batches++;
        _events += events;
        _rowsBulkInserted += rowsBulkInserted;
        _rowsViaEntityFramework += rowsViaEntityFramework;
        _handlerMilliseconds += (long)handlerDuration.TotalMilliseconds;
        _bulkCopyMilliseconds += (long)bulkCopyDuration.TotalMilliseconds;
        _saveChangesMilliseconds += (long)saveChangesDuration.TotalMilliseconds;

        var elapsed = _sinceStart.Elapsed;
        var windowElapsed = elapsed - _elapsedAtLastLog;

        // Either trigger is enough. Batches alone go quiet exactly when the projection is slowest, which is when the
        // line is worth having; minutes alone would spam a projection that is keeping up at the tail.
        if (_batches - _batchesAtLastLog < _logIntervalInBatches && windowElapsed < _logInterval)
        {
            return;
        }

        var windowBatches = _batches - _batchesAtLastLog;
        var windowEvents = _events - _eventsAtLastLog;
        var eventsPerSecond = windowElapsed.TotalSeconds > 0 ? windowEvents / windowElapsed.TotalSeconds : 0;
        var millisecondsPerEvent = windowEvents > 0 ? windowElapsed.TotalMilliseconds / windowEvents : 0;

        _logger.LogInformation(
            "{Projection} {Mode}: {WindowEvents} event(s) in {WindowBatches} batch(es) over {WindowSeconds:F1}s - " +
            "{EventsPerSecond:F2} events/s, {MillisecondsPerEvent:F0}ms per event. " +
            "This window: {WindowHandlerMs}ms handlers, {WindowBulkMs}ms bulk copy, {WindowSaveMs}ms SaveChanges, " +
            "{WindowBulkRows} row(s) bulk, {WindowEfRows} row(s) via EF. " +
            "Since start: {Events} event(s) in {Batches} batch(es).",
            _projectionName,
            isCatchingUp ? "catching up" : "live",
            windowEvents,
            windowBatches,
            windowElapsed.TotalSeconds,
            eventsPerSecond,
            millisecondsPerEvent,
            _handlerMilliseconds - _handlerMillisecondsAtLastLog,
            _bulkCopyMilliseconds - _bulkCopyMillisecondsAtLastLog,
            _saveChangesMilliseconds - _saveChangesMillisecondsAtLastLog,
            _rowsBulkInserted - _rowsBulkInsertedAtLastLog,
            _rowsViaEntityFramework - _rowsViaEntityFrameworkAtLastLog,
            _events,
            _batches);

        _batchesAtLastLog = _batches;
        _eventsAtLastLog = _events;
        _rowsBulkInsertedAtLastLog = _rowsBulkInserted;
        _rowsViaEntityFrameworkAtLastLog = _rowsViaEntityFramework;
        _handlerMillisecondsAtLastLog = _handlerMilliseconds;
        _bulkCopyMillisecondsAtLastLog = _bulkCopyMilliseconds;
        _saveChangesMillisecondsAtLastLog = _saveChangesMilliseconds;
        _elapsedAtLastLog = elapsed;
    }
}
