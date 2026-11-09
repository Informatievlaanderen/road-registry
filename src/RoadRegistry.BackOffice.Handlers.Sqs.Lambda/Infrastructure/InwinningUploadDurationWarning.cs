namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Microsoft.Extensions.Logging;

// Warns when processing an inwinning upload is still running after 8, 9 and 10 minutes, so long uploads can be spotted
// before they run into the lambda's timeout. The warning is logged as an error so it reaches Slack through the Slack
// sink, and says in its text that it is a warning. Nothing is logged once it is disposed, so an upload that finishes in
// time leaves no trace.
public sealed class InwinningUploadDurationWarning : IDisposable
{
    public static readonly IReadOnlyCollection<TimeSpan> Thresholds =
    [
        TimeSpan.FromMinutes(8),
        TimeSpan.FromMinutes(9),
        TimeSpan.FromMinutes(10)
    ];

    private readonly ITimer[] _timers;

    public InwinningUploadDurationWarning(ILogger logger, string action, DownloadId downloadId, UploadId uploadId, string? nisCode = null, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        timeProvider ??= TimeProvider.System;

        var downloadIdAsString = downloadId.ToString();
        var uploadIdAsString = uploadId.ToString();

        _timers = Thresholds
            .Select(threshold => timeProvider.CreateTimer(
                _ => LogWarning(logger, action, (int)threshold.TotalMinutes, downloadIdAsString, uploadIdAsString, nisCode),
                null,
                threshold,
                Timeout.InfiniteTimeSpan))
            .ToArray();
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
