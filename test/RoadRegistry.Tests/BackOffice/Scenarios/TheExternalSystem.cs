namespace RoadRegistry.Tests.BackOffice.Scenarios;

using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

public static class TheExternalSystem
{
    public static Command PutsInARoadNetworkExtractRequest(ExternalExtractRequestId requestId,
        DownloadId downloadId,
        ExtractDescription extractDescription,
        RoadNetworkExtractGeometry contour)
    {
        return new Command(new RequestRoadNetworkExtract
        {
            ExternalRequestId = requestId,
            DownloadId = downloadId,
            Description = extractDescription,
            Contour = contour
        });
    }

    public static Command UploadsRoadNetworkExtractChangesArchive(
        ExtractRequestId requestId,
        DownloadId downloadId,
        UploadId uploadId,
        ArchiveId archiveId,
        TicketId? ticketId = null)
    {
        return new Command(new UploadRoadNetworkExtractChangesArchive
        {
            RequestId = requestId,
            DownloadId = downloadId,
            UploadId = uploadId,
            ArchiveId = archiveId,
            TicketId = ticketId
        });
    }
}
