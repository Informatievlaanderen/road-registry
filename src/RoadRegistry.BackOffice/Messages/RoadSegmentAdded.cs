namespace RoadRegistry.BackOffice.Messages;

public class RoadSegmentAdded
{
    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public int EndNodeId { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public string GeometryDrawMethod { get; set; }
    public int GeometryVersion { get; set; }
    public int Id { get; set; }
    public RoadSegmentLaneAttributes[] Lanes { get; set; }
    public RoadSegmentSideAttributes LeftSide { get; set; }
    public MaintenanceAuthority MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public RoadSegmentSideAttributes RightSide { get; set; }
    public int StartNodeId { get; set; }
    public string Status { get; set; }
    public RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    public int TemporaryId { get; set; }
    public int Version { get; set; }
    public RoadSegmentWidthAttributes[] Widths { get; set; }
}