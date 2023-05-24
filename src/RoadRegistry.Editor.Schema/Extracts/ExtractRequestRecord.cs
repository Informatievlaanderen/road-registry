namespace RoadRegistry.Editor.Schema.Extracts;

using System;
using NetTopologySuite.Geometries;

public class ExtractRequestRecord
{
    public Guid DownloadId { get; set; }
    public string Description { get; set; }
    public Geometry Contour { get; set; }

    public string RequestId { get; set; }
    public long RequestedOn { get; set; }
    public string ExternalRequestId { get; set; }
    public bool UploadExpected { get; set; }
}
