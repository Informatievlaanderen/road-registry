namespace RoadRegistry.GradeSeparatedJunction.Changes;

using ScopedRoadNetwork;

public sealed record ModifyGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId? LowerRoadSegmentId { get; init; }
    public RoadSegmentId? UpperRoadSegmentId { get; init; }
    public GradeSeparatedJunctionTypeV2? Type { get; init; }
}
