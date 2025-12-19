namespace RoadRegistry.GradeSeparatedJunction.Changes;

using RoadNetwork;

public sealed record ModifyGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId? LowerRoadSegmentId { get; init; }
    public RoadSegmentId? UpperRoadSegmentId { get; init; }
    public GradeSeparatedJunctionType? Type { get; init; }
}
