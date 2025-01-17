namespace RoadRegistry.Wms.Schema;

using System;
using NetTopologySuite.Geometries;

public class ExtractRequestRecord
{
    public Guid DownloadId { get; set; }
    public string Description { get; set; }
    public Geometry Contour { get; set; }
    public string ExternalRequestId { get; set; }
    public DateTimeOffset RequestedOn { get; set; }
    public bool IsInformative { get; set; }
    public DateTimeOffset? DownloadedOn { get; set; }
}
