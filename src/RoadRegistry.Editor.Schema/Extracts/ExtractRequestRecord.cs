namespace RoadRegistry.Editor.Schema.Extracts;

using System;

public class ExtractRequestRecord
{
    public Guid DownloadId { get; set; }
    public string Description { get; set; }
    public string Contour { get; set; }

    public string RequestId { get; set; }
    public long RequestedOn { get; set; }
    public string ExternalRequestId { get; set; }

    public bool Available { get; set; }
    public long AvailableOn { get; set; }

    public bool UploadExpected { get; set; }
}
