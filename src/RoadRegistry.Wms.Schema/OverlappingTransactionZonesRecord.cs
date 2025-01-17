namespace RoadRegistry.Wms.Schema;

using System;
using NetTopologySuite.Geometries;

public class OverlappingTransactionZonesRecord
{
    public int Id { get; set; }
    public Geometry Contour { get; set; }
    public Guid DownloadId1 { get; set; }
    public Guid DownloadId2 { get; set; }
    public string? Description1 { get; set; }
    public string? Description2 { get; set; }
    public string Label { get; set; }
}
