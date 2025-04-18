namespace RoadRegistry.BackOffice.Messages;

using System;

public class UploadRoadNetworkExtractChangesArchive
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string RequestId { get; set; }
    public string ZipArchiveWriterVersion { get; set; }
    public Guid? TicketId { get; set; }

    public Guid UploadId { get; set; }
}
