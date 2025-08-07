namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using BackOffice.Extracts;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Extracts.Schema;

public class ExtractsCloser : IExtractsCloser
{
    private readonly ExtractsDbContext _extractsDbContext;

    public ExtractsCloser(ExtractsDbContext extractsDbContext)
    {
        _extractsDbContext = extractsDbContext;
    }

    public async Task CloseAsync(DownloadId downloadId, CancellationToken cancellationToken)
    {
        var record = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        if (record is null)
        {
            throw new UploadExtractNotFoundException($"Could not close extract with downloadId {downloadId}");
        }

        record.Closed = true;

        await _extractsDbContext.SaveChangesAsync(cancellationToken);
    }
}
