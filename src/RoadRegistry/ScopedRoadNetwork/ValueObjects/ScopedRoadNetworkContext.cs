namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.ValueObjects;

public class ScopedRoadNetworkContext
{
    public ScopedRoadNetwork RoadNetwork { get; }
    public IIdentifierTranslator IdTranslator { get; }
    public Provenance Provenance { get; }
    public ILogger Logger { get; }
    public VerificationContextTolerances Tolerances => VerificationContextTolerances.Cm;

    public ScopedRoadNetworkContext(ScopedRoadNetwork roadNetwork, IIdentifierTranslator idTranslator, Provenance provenance, ILogger? logger = null)
    {
        RoadNetwork = roadNetwork;
        IdTranslator = idTranslator;
        Provenance = provenance;
        Logger = logger ?? NullLogger.Instance;
    }
}
