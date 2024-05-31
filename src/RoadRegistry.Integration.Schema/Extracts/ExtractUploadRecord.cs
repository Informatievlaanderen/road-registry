namespace RoadRegistry.Integration.Schema.Extracts;

using System;

public class ExtractUploadRecord
{
    public string ArchiveId { get; set; }
    public string ChangeRequestId { get; set; }
    public long CompletedOn { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public long ReceivedOn { get; set; }
    public string RequestId { get; set; }
    public ExtractUploadStatus Status { get; set; }
    public Guid UploadId { get; set; }
}