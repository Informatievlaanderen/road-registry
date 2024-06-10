namespace RoadRegistry.Integration.Schema.RoadSegments;

using System;
using BackOffice;
using NetTopologySuite.Geometries;

public class RoadSegmentLatestItemEuropeanRoadAttribute
{
    public int Id { get; set; }
    public int RoadSegmentId { get; set; }


    
    public bool IsRemoved { get; set; }
}
