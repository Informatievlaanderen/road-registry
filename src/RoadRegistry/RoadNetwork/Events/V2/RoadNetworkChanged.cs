namespace RoadRegistry.RoadNetwork.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public record RoadNetworkChanged : IMartenEvent
{
    public RoadNetworkChangeGeometry? ScopeGeometry { get; init; }
    public DownloadId? DownloadId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
