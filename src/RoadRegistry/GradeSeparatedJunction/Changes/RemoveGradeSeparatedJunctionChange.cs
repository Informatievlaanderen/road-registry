namespace RoadRegistry.GradeSeparatedJunction.Changes;

using RoadNetwork;
using RoadRegistry.BackOffice;

public sealed record RemoveGradeSeparatedJunctionChange : IRoadNetworkChange
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
