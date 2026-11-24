namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Microsoft.Extensions.Logging;

// Warns when processing an inwinning upload is still running after 8, 9 and 10 minutes, and every minute after that, so
// long uploads can be spotted before they run into the lambda's timeout, and one that keeps running stays in sight. The
// warning is logged as an error so it reaches Slack through the Slack sink, and says in its text that it is a warning.
// Nothing is logged once it is disposed, so an upload that finishes in time leaves no trace.
public sealed class InwinningUploadDurationWarning : IDisposable
{
    public static readonly IReadOnlyCollection<TimeSpan> Thresholds =
    [
        TimeSpan.FromMinutes(8),
        TimeSpan.FromMinutes(9),
        TimeSpan.FromMinutes(10)
    ];

    // From the last threshold on, the warning is repeated this often for as long as it runs.
    public static readonly TimeSpan RepeatInterval = TimeSpan.FromMinutes(1);

    private readonly ITimer[] _timers;

    public InwinningUploadDurationWarning(ILogger logger, string action, DownloadId downloadId, UploadId uploadId, string? nisCode = null, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        timeProvider ??= TimeProvider.System;

        var downloadIdAsString = downloadId.ToString();
        var uploadIdAsString = uploadId.ToString();

        var lastThreshold = Thresholds.Max();

        _timers = Thresholds
            .Where(threshold => threshold != lastThreshold)
            .Select(threshold => timeProvider.CreateTimer(
                _ => LogWarning(logger, action, (int)threshold.TotalMinutes, downloadIdAsString, uploadIdAsString, nisCode),
                null,
                threshold,
                Timeout.InfiniteTimeSpan))
            .Append(CreateRepeatingTimer())
            .ToArray();

        ITimer CreateRepeatingTimer()
        {
            var repeats = -1;
            return timeProvider.CreateTimer(
                _ =>
                {
                    var elapsed = lastThreshold + RepeatInterval * Interlocked.Increment(ref repeats);
                    LogWarning(logger, action, (int)elapsed.TotalMinutes, downloadIdAsString, uploadIdAsString, nisCode);
                },
                null,
                lastThreshold,
                RepeatInterval);
        }
    }

    private static void LogWarning(ILogger logger, string action, int minutes, string downloadId, string uploadId, string? nisCode)
    {
        if (nisCode is null)
        {
            logger.LogError(
                "Warning: {Action} is na {Minutes} minuten nog bezig: DownloadId {DownloadId}, UploadId {UploadId}",
                action, minutes, downloadId, uploadId);
            return;
        }

        logger.LogError(
            "Warning: {Action} is na {Minutes} minuten nog bezig: DownloadId {DownloadId}, UploadId {UploadId}, NisCode {NisCode}",
            action, minutes, downloadId, uploadId, nisCode);
    }

    public void Dispose()
    {
        foreach (var timer in _timers)
        {
            timer.Dispose();
        }
    }
}
