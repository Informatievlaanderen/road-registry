namespace RoadRegistry.RoadSegment.Changes;

using RoadRegistry.ValueObjects;

// 'Wijzig geometriemethode': only the draw method changes, so a dedicated change carries just that. It is applied as
// a regular road segment modification with everything else left untouched. It never travels through the generic
// ordered road network changes, so it is deliberately not an IRoadNetworkChange.
public sealed record ChangeRoadSegmentGeometryDrawMethodChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometryDrawMethodV2 GeometryDrawMethod { get; init; }
}
