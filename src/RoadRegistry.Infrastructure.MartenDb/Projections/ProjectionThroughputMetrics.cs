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
    private readonly Stopwatch _sinceStart = Stopwatch.StartNew();

    private long _batches;
    private long _events;
    private long _rowsBulkInserted;
    private long _rowsViaEntityFramework;
    private long _handlerMilliseconds;
    private long _bulkCopyMilliseconds;
    private long _saveChangesMilliseconds;

    private long _eventsAtLastLog;
    private TimeSpan _elapsedAtLastLog = TimeSpan.Zero;

    public ProjectionThroughputMetrics(ILogger logger, string projectionName, int logIntervalInBatches)
    {
        _logger = logger;
        _projectionName = projectionName;
        _logIntervalInBatches = Math.Max(1, logIntervalInBatches);
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

        if (_batches % _logIntervalInBatches != 0)
        {
            return;
        }

        var elapsed = _sinceStart.Elapsed;
        var windowElapsed = elapsed - _elapsedAtLastLog;
        var windowEvents = _events - _eventsAtLastLog;
        var eventsPerSecond = windowElapsed.TotalSeconds > 0 ? windowEvents / windowElapsed.TotalSeconds : 0;

        _logger.LogInformation(
            "{Projection} {Mode}: {Events} events in {Batches} batches ({EventsPerSecond:F1} events/s over the last {WindowBatches} batches). " +
            "Rows written: {BulkRows} bulk, {EfRows} via EF. Time: {HandlerMs}ms handlers, {BulkMs}ms bulk copy, {SaveMs}ms SaveChanges.",
            _projectionName,
            isCatchingUp ? "catching up" : "live",
            _events,
            _batches,
            eventsPerSecond,
            _logIntervalInBatches,
            _rowsBulkInserted,
            _rowsViaEntityFramework,
            _handlerMilliseconds,
            _bulkCopyMilliseconds,
            _saveChangesMilliseconds);

        _eventsAtLastLog = _events;
        _elapsedAtLastLog = elapsed;
    }
}
