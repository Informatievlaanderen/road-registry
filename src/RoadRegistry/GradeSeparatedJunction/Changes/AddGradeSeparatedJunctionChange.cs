namespace RoadRegistry.GradeSeparatedJunction.Changes;

using RoadNetwork;
using RoadRegistry.RoadSegment.ValueObjects;

public sealed record AddGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId TemporaryId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionType Type { get; init; }
}
