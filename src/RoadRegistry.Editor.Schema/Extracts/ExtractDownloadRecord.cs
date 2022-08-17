namespace RoadRegistry.Editor.Schema.Extracts;

using System;

public class ExtractDownloadRecord
{
    public Guid DownloadId { get; set; }
    public string ExternalRequestId { get; set; }
    public string RequestId { get; set; }
    public string ArchiveId { get; set; }
    public long RequestedOn { get; set; }
    public bool Available { get; set; }
    public long AvailableOn { get; set; }
}
