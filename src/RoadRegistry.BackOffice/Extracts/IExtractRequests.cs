namespace RoadRegistry.BackOffice.Extracts;

using System.Threading;
using System.Threading.Tasks;

public interface IExtractRequests
{
    Task UploadAcceptedAsync(DownloadId downloadId, CancellationToken cancellationToken);
    Task UploadRejectedAsync(DownloadId downloadId, CancellationToken cancellationToken);
}
