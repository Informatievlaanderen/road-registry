namespace RoadRegistry.Wms.Schema;

using System;
using NetTopologySuite.Geometries;

public class TransactionZoneRecord
{
    public Guid DownloadId { get; set; }
    public string? Description { get; set; }
    public Geometry Contour { get; set; }
    public string ExternalRequestId { get; set; }
    public string Label { get; set; }
}
