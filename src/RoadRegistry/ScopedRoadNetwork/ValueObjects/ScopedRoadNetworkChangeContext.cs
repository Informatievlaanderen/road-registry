namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.ValueObjects;

public class ScopedRoadNetworkChangeContext
{
    public ScopedRoadNetwork RoadNetwork { get; }
    public IIdentifierTranslator IdTranslator { get; }
    public Provenance Provenance { get; }
    public RoadNetworkChangesSummary Summary { get; }
    public ILogger Logger { get; }
    public VerificationContextTolerances Tolerances => VerificationContextTolerances.Cm;

    public ScopedRoadNetworkChangeContext(ScopedRoadNetwork roadNetwork, IIdentifierTranslator idTranslator, Provenance provenance, ILogger? logger = null)
    {
        RoadNetwork = roadNetwork;
        IdTranslator = idTranslator;
        Provenance = provenance;
        Summary = new RoadNetworkChangesSummary();
        Logger = logger ?? NullLogger.Instance;
    }
}
