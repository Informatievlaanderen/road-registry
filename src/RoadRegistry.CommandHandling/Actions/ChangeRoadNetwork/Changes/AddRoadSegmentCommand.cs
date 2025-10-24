namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using BackOffice.Messages;
using RoadNetwork.ValueObjects;
using RoadSegment.ValueObjects;

public class AddRoadSegmentCommand
{
    public int TemporaryId { get; set; }
    public int? OriginalId { get; set; }
    public int? PermanentId { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public GeometryObject Geometry { get; set; }
    public string AccessRestriction { get; set; }
    public string Category { get; set; }
    public string GeometryDrawMethod { get; set; }
    public string MaintenanceAuthority { get; set; }
    public string Morphology { get; set; }
    public string Status { get; set; }
    public int? LeftSideStreetNameId { get; set; }
    public int? RightSideStreetNameId { get; set; }
    public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
}
