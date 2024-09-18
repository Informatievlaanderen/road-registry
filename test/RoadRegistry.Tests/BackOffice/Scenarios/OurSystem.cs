namespace RoadRegistry.Tests.BackOffice.Scenarios;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

public static class OurSystem
{
    public static Command AnnouncesRoadNetworkExtractDownloadBecameAvailable(
        ExtractRequestId requestId,
        DownloadId downloadId,
        ArchiveId archiveId,
        ICollection<Guid> overlapsWithDownloadIds = null)
    {
        return new Command(new AnnounceRoadNetworkExtractDownloadBecameAvailable
        {
            RequestId = requestId,
            DownloadId = downloadId,
            ArchiveId = archiveId,
            OverlapsWithDownloadIds = overlapsWithDownloadIds ?? []
        });
    }

    public static Command AnnouncesRoadNetworkExtractDownloadTimeoutOccurred(ExtractRequestId requestId, DownloadId downloadId)
    {
        return new Command(new AnnounceRoadNetworkExtractDownloadTimeoutOccurred
        {
            RequestId = requestId,
            DownloadId = downloadId
        });
    }
}
