namespace RoadRegistry.RoadSegment.Events;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentModified: IHaveHash
{
    public const string EventName = "RoadSegmentModified";

    public required int Id { get; init; }
    public required int? OriginalId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required int StartNodeId { get; init; }
    public required int EndNodeId { get; init; }
    public required string AccessRestriction { get; init; }
    public required string Category { get; init; }
    public required string GeometryDrawMethod { get; init; }
    public required string MaintenanceAuthorityId { get; init; }
    public required string Morphology { get; init; }
    public required string Status { get; init; }
    public required RoadSegmentSideAttribute LeftSide { get; init; }
    public required RoadSegmentSideAttribute RightSide { get; init; }
    public required RoadSegmentLaneAttribute[] Lanes { get; init; }
    public required RoadSegmentSurfaceAttribute[] Surfaces { get; init; }
    public required RoadSegmentWidthAttribute[] Widths { get; init; }
    //public required bool ConvertedFromOutlined { get; init; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
