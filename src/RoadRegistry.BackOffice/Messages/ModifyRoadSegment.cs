namespace RoadRegistry.BackOffice.Messages;

public class ModifyRoadSegment
{
    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public bool CategoryModified { get; set; }
    public int EndNodeId { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public string GeometryDrawMethod { get; set; }
    public int Id { get; set; }
    public int? OriginalId { get; set; }
    public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public int? LeftSideStreetNameId { get; set; }
    public string MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public int? RightSideStreetNameId { get; set; }
    public int StartNodeId { get; set; }
    public string Status { get; set; }
    public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
    public int? Version { get; set; }
    public int? GeometryVersion { get; set; }
    //public bool ConvertedFromOutlined { get; set; }
    public string? PreviousGeometryDrawMethod { get; set; }
    //public bool ConvertedToOutlined { get; set; }
}
