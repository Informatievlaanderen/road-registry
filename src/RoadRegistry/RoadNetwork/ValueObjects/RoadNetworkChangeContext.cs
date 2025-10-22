namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice.Core;

public class RoadNetworkChangeContext
{
    public required VerificationContextTolerances Tolerances { get; init; }
    public required RoadNetwork RoadNetwork { get; init; }
    public required IRoadNetworkIdGenerator IdGenerator { get; init; }
}
