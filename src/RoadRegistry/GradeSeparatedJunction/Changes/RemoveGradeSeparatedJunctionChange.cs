namespace RoadRegistry.GradeSeparatedJunction.Changes;

using ScopedRoadNetwork;

public sealed record RemoveGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
