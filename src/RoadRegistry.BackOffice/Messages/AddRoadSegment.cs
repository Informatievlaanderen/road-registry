namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegment
{
    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public int EndNodeId { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public string GeometryDrawMethod { get; set; }
    public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public int? LeftSideStreetNameId { get; set; }
    public string MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public int? RightSideStreetNameId { get; set; }
    public int StartNodeId { get; set; }
    public string Status { get; set; }
    public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public int TemporaryId { get; set; }
    public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
}