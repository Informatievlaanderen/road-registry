namespace RoadRegistry.Editor.Schema.Extracts;

using System;

public class ExtractUploadRecord
{
    public Guid UploadId { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public string ArchiveId { get; set; }
    public string ChangeRequestId { get; set; }
    public long ReceivedOn { get; set; }
    public ExtractUploadStatus Status { get; set; }
    public long CompletedOn { get; set; }
}
