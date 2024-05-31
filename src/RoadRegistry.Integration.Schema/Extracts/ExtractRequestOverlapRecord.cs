namespace RoadRegistry.Integration.Schema.Extracts;

using System;
using NetTopologySuite.Geometries;

public class ExtractRequestOverlapRecord
{
    public int Id { get; set; }
    public Geometry Contour { get; set; }
    public Guid DownloadId1 { get; set; }
    public Guid DownloadId2 { get; set; }
    public string Description1 { get; set; }
    public string Description2 { get; set; }
}
