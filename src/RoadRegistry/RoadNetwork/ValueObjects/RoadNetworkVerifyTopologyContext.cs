namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice.Core;

public class RoadNetworkVerifyTopologyContext
{
    public required RoadNetwork RoadNetwork { get; init; }
    public required IIdentifierTranslator IdTranslator { get; init; }

    public VerificationContextTolerances Tolerances => VerificationContextTolerances.Default;
}
