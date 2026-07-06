namespace RoadRegistry.ScopedRoadNetwork.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using ValueObjects;

public record RoadNetworkWasChangedBecauseOfExtract : IMartenEvent, ICreatedEvent
{
    public const string EventName = "RoadNetworkWasChangedBecauseOfExtract"; // BE CAREFUL CHANGING THIS!!

    public required ScopedRoadNetworkId RoadNetworkId { get; init; }
    public RoadNetworkChangeGeometry? ScopeGeometry { get; init; }
    public DownloadId? DownloadId { get; init; }
    public required RoadNetworkChangedSummary Summary { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
