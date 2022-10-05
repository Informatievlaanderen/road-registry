using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegment : IMessage
{
    public int TemporaryId { get; set; }
    public int StartNodeId { get; set; }
    public int EndNodeId { get; set; }
    public RoadSegmentGeometry Geometry { get; set; }
    public string MaintenanceAuthority { get; set; }
    public string GeometryDrawMethod { get; set; }
    public string Morphology { get; set; }
    public string Status { get; set; }
    public string Category { get; set; }
    public string AccessRestriction { get; set; }
    public int? LeftSideStreetNameId { get; set; }
    public int? RightSideStreetNameId { get; set; }
    public RequestedRoadSegmentLaneAttribute[] Lanes { get; set; }
    public RequestedRoadSegmentWidthAttribute[] Widths { get; set; }
    public RequestedRoadSegmentSurfaceAttribute[] Surfaces { get; set; }
}
