namespace RoadRegistry.BackOffice.Scenarios;

using Framework;
using Messages;

public static class OurSystem
{
    public static Command AnnouncesRoadNetworkExtractDownloadBecameAvailable(
        ExtractRequestId requestId,
        DownloadId downloadId,
        ArchiveId archiveId)
    {
        return new Command(new AnnounceRoadNetworkExtractDownloadBecameAvailable
        {
            RequestId = requestId,
            DownloadId = downloadId,
            ArchiveId = archiveId
        });
    }
}
