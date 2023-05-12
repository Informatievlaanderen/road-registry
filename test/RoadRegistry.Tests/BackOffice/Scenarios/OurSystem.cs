namespace RoadRegistry.Tests.BackOffice.Scenarios;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

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

    public static Command AnnouncesRoadNetworkExtractDownloadTimeoutOccurred(ExtractRequestId requestId)
    {
        return new Command(new AnnounceRoadNetworkExtractDownloadTimeoutOccurred
        {
            RequestId = requestId
        });
    }
}