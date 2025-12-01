namespace RoadRegistry.GradeSeparatedJunction.Changes;

using RoadNetwork;

public sealed record RemoveGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
