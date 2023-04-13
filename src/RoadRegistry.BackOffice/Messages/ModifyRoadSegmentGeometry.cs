namespace RoadRegistry.BackOffice.Messages;

public class ModifyRoadSegmentGeometry
{
    public int Id { get; set; }
    public string GeometryDrawMethod { get; set; }
    
    public RoadSegmentGeometry Geometry { get; set; }
    public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
}
