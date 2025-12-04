namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using BackOffice.Extracts;
using CommandHandling.Extracts;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts.Schema;

public class ExtractRequests : IExtractRequests
{
    private readonly ExtractsDbContext _extractsDbContext;

    public ExtractRequests(ExtractsDbContext extractsDbContext)
    {
        _extractsDbContext = extractsDbContext;
    }

    public async Task UploadAcceptedAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        await UpdateExtractDownload(downloadId, record =>
        {
            record.UploadStatus = ExtractUploadStatus.Accepted;
            record.Closed = true;
        }, cancellationToken);
    }

    public async Task UploadRejectedAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        await UpdateExtractDownload(downloadId, record =>
        {
            record.UploadStatus = ExtractUploadStatus.Rejected;
        }, cancellationToken);
    }

    private async Task UpdateExtractDownload(DownloadId downloadId, Action<ExtractDownload> change, CancellationToken cancellationToken)
    {
        var record = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        if (record is null)
        {
            throw new UploadExtractNotFoundException($"Could not close extract with downloadId {downloadId}");
        }

        change(record);

        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }
}
