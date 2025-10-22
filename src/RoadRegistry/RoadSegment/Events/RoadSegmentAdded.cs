namespace RoadRegistry.RoadSegment.Events;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentAdded: IHaveHash
{
    public const string EventName = "RoadSegmentAdded";

    public required int Id { get; set; }
    public required int? OriginalId { get; set; }
    public required RoadSegmentGeometry Geometry { get; set; }
    public required int StartNodeId { get; set; }
    public required int EndNodeId { get; set; }
    public required string AccessRestriction { get; set; }
    public required string Category { get; set; }
    public required string GeometryDrawMethod { get; set; }
    public required string MaintenanceAuthorityId { get; set; }
    public required string Morphology { get; set; }
    public required string Status { get; set; }
    public required RoadSegmentSideAttribute LeftSide { get; set; }
    public required RoadSegmentSideAttribute RightSide { get; set; }
    public required RoadSegmentLaneAttribute[] Lanes { get; set; }
    public required RoadSegmentSurfaceAttribute[] Surfaces { get; set; }
    public required RoadSegmentWidthAttribute[] Widths { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
