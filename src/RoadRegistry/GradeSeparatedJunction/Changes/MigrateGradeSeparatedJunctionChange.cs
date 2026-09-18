namespace RoadRegistry.GradeSeparatedJunction.Changes;

using ScopedRoadNetwork;

public sealed record MigrateGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public required RoadSegmentId LowerRoadSegmentId { get; init; }
    public required RoadSegmentId UpperRoadSegmentId { get; init; }
    public required GradeSeparatedJunctionTypeV2 Type { get; init; }
}
