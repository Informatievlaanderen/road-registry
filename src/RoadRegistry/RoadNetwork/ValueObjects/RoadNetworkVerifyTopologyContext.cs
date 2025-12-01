namespace RoadRegistry.RoadNetwork.ValueObjects;

using RoadRegistry.ValueObjects;

public class RoadNetworkVerifyTopologyContext
{
    public required RoadNetwork RoadNetwork { get; init; }
    public required IIdentifierTranslator IdTranslator { get; init; }

    public VerificationContextTolerances Tolerances => VerificationContextTolerances.Default;
}
