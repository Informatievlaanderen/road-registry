namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using RoadRegistry.ValueObjects;

public class RoadNetworkVerifyTopologyContext
{
    public required ScopedRoadNetwork RoadNetwork { get; init; }
    public required IIdentifierTranslator IdTranslator { get; init; }

    public VerificationContextTolerances Tolerances => VerificationContextTolerances.Default;
}
