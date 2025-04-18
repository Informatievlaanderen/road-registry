namespace RoadRegistry.BackOffice.Messages;

using System;

public class UploadRoadNetworkChangesArchive
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string ExtractRequestId { get; set; }
    public string ZipArchiveWriterVersion { get; set; }
    public Guid? TicketId { get; set; }
}
