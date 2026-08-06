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
    public IEventOrdinalProvider OrdinalProvider { get; }

    // The identifier translator is owned by the context rather than passed in: it maps the temporary identifiers of
    // one change onto the permanent ones handed out while applying it, so it only ever makes sense per change - which
    // is exactly the lifetime of this context.
    public ScopedRoadNetworkChangeContext(ScopedRoadNetwork roadNetwork, Provenance provenance, ILogger? logger = null)
    {
        RoadNetwork = roadNetwork;
        IdTranslator = new IdentifierTranslator();
        Provenance = provenance;
        Summary = new RoadNetworkChangesSummary();
        Logger = logger ?? NullLogger.Instance;
        OrdinalProvider = new EventOrdinalProvider();
        roadNetwork.AttachOrdinalProvider(OrdinalProvider);
    }
}
