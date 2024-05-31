namespace RoadRegistry.Integration.Schema.Extracts;

using System;

public class ExtractDownloadRecord
{
    public string ArchiveId { get; set; }
    public bool Available { get; set; }
    public long AvailableOn { get; set; }
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public long RequestedOn { get; set; }
    public string RequestId { get; set; }
    public bool IsInformative { get; set; }
    public DateTimeOffset? DownloadedOn { get; set; }
}
