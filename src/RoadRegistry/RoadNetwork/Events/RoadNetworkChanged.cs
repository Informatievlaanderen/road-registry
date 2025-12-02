namespace RoadRegistry.RoadNetwork.Events;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record RoadNetworkChanged : IMartenEvent
{
    public GeometryObject? ScopeGeometry { get; init; }
    public DownloadId? DownloadId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public RoadNetworkChanged()
    {
    }
    protected RoadNetworkChanged(RoadNetworkChanged other) // Needed for Marten
    {
    }
}
