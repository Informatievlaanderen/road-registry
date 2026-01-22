namespace RoadRegistry.GradeSeparatedJunction.Changes;

using RoadRegistry.RoadSegment.ValueObjects;
using ScopedRoadNetwork;

public sealed record AddGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId TemporaryId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionTypeV2 Type { get; init; }
}
