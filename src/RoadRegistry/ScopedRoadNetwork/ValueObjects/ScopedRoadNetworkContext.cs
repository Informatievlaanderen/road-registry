namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public class ScopedRoadNetworkContext
{
    public ScopedRoadNetwork RoadNetwork { get; }
    public IIdentifierTranslator IdTranslator { get; }
    public Provenance Provenance { get; }
    public VerificationContextTolerances Tolerances => VerificationContextTolerances.Cm;

    public ScopedRoadNetworkContext(ScopedRoadNetwork roadNetwork, IIdentifierTranslator idTranslator, Provenance provenance)
    {
        RoadNetwork = roadNetwork;
        IdTranslator = idTranslator;
        Provenance = provenance;
    }
}
