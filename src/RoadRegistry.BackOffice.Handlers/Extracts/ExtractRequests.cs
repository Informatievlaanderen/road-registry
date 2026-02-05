namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;

public class ExtractRequests : IExtractRequests
{
    private readonly ExtractsDbContext _extractsDbContext;

    public ExtractRequests(ExtractsDbContext extractsDbContext)
    {
        _extractsDbContext = extractsDbContext;
    }

    public async Task UploadAcceptedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, async record =>
        {
            record.Status = ExtractUploadStatus.Accepted;

            var extractDownload = await _extractsDbContext.ExtractDownloads.SingleAsync(x => x.DownloadId == record.DownloadId, cancellationToken);
            extractDownload.Closed = true;
        }, cancellationToken);
    }

    public async Task AutomaticValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, record =>
        {
            record.Status = ExtractUploadStatus.AutomaticValidationFailed;
            return Task.CompletedTask;
        }, cancellationToken);
    }

    public async Task ManualValidationFailedAsync(UploadId uploadId, CancellationToken cancellationToken)
    {
        await UpdateExtractUpload(uploadId, record =>
        {
            record.Status = ExtractUploadStatus.ManualValidationFailed;
            return Task.CompletedTask;
        }, cancellationToken);
    }

    private async Task UpdateExtractUpload(UploadId uploadId, Func<ExtractUpload, Task> change, CancellationToken cancellationToken)
    {
        var record = await _extractsDbContext.ExtractUploads.SingleOrDefaultAsync(x => x.UploadId == uploadId.ToGuid(), cancellationToken);
        if (record is null)
        {
            throw new UploadExtractNotFoundException($"Could find extractupload with uploadId {uploadId}");
        }

        await change(record);

        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }
}
