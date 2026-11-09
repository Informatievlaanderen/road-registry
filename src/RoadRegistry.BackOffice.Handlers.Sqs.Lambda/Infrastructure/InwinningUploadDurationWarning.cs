namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.Schema;

// Logs a warning when processing an inwinning upload is still running after 8, 9 and 10 minutes, so long uploads can be
// spotted before they run into the lambda's timeout. Nothing is logged once it is disposed, so an upload that finishes
// in time leaves no trace.
public sealed class InwinningUploadDurationWarning : IDisposable
{
    public static readonly IReadOnlyCollection<TimeSpan> Thresholds =
    [
        TimeSpan.FromMinutes(8),
        TimeSpan.FromMinutes(9),
        TimeSpan.FromMinutes(10)
    ];

    private readonly ITimer[] _timers;

    public InwinningUploadDurationWarning(ILogger logger, string action, InwinningUploadDescription upload, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(upload);

        timeProvider ??= TimeProvider.System;

        _timers = Thresholds
            .Select(threshold => timeProvider.CreateTimer(
                _ => logger.LogWarning(
                    "{Action} is na {Minutes} minuten nog bezig: DownloadId {DownloadId}, UploadId {UploadId}, NisCode {NisCode}, Beschrijving {Beschrijving}",
                    action,
                    (int)threshold.TotalMinutes,
                    upload.DownloadId,
                    upload.UploadId,
                    upload.NisCode,
                    upload.Description),
                null,
                threshold,
                Timeout.InfiniteTimeSpan))
            .ToArray();
    }

    public static async Task<InwinningUploadDurationWarning> Start(
        ILogger logger,
        string action,
        ExtractsDbContext extractsDbContext,
        DownloadId downloadId,
        UploadId uploadId,
        CancellationToken cancellationToken,
        TimeProvider? timeProvider = null)
    {
        var upload = await InwinningUploadDescription.Find(extractsDbContext, downloadId, uploadId, logger, cancellationToken);
        return new InwinningUploadDurationWarning(logger, action, upload, timeProvider);
    }

    public void Dispose()
    {
        foreach (var timer in _timers)
        {
            timer.Dispose();
        }
    }
}

public sealed record InwinningUploadDescription(DownloadId DownloadId, UploadId UploadId, string? NisCode, string? Description)
{
    // What identifies the extract to whoever reads the log. Looking it up must never stop the upload itself, so a
    // failure leaves the identifiers it could not find empty.
    public static async Task<InwinningUploadDescription> Find(ExtractsDbContext extractsDbContext, DownloadId downloadId, UploadId uploadId, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var downloadIdGuid = downloadId.ToGuid();

            var nisCode = await extractsDbContext.Inwinningszones
                .Where(x => x.DownloadId == downloadIdGuid)
                .Select(x => x.NisCode)
                .FirstOrDefaultAsync(cancellationToken);

            var description = await (
                    from download in extractsDbContext.ExtractDownloads
                    join request in extractsDbContext.ExtractRequests on download.ExtractRequestId equals request.ExtractRequestId
                    where download.DownloadId == downloadIdGuid
                    select request.Description)
                .FirstOrDefaultAsync(cancellationToken);

            return new InwinningUploadDescription(downloadId, uploadId, nisCode, description);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not look up the extract of inwinning upload {UploadId} (DownloadId {DownloadId})", uploadId, downloadId);
            return new InwinningUploadDescription(downloadId, uploadId, null, null);
        }
    }
}
