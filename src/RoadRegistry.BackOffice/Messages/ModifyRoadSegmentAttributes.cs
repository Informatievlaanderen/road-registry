namespace RoadRegistry.BackOffice.Messages;

public class ModifyRoadSegmentAttributes
{
    public int Id { get; set; }
    public string GeometryDrawMethod { get; set; }

    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public string MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public string Status { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
}
