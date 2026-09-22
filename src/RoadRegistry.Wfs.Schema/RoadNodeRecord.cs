namespace RoadRegistry.Wfs.Schema;

using System;
using NetTopologySuite.Geometries;

public class RoadNodeRecord
{
    public DateTime? BeginTime { get; set; }
    public Geometry Geometry { get; set; }
    public int Id { get; set; }
    public string Type { get; set; }
    // Set once the road node is ingewonnen (migrated to V2): the view that serves this table leaves it out from then on.
    public bool IsV2 { get; set; }
}