namespace RoadRegistry.Wfs.Schema;

using System;
using NetTopologySuite.Geometries;

public class RoadNodeRecord
{
    public DateTime? BeginTime { get; set; }
    public Geometry Geometry { get; set; }
    public int Id { get; set; }
    public string Type { get; set; }
}
