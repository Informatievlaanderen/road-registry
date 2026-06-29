namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record RoadNodeWasMigrated : IMartenEvent
{
    public const string EventName = "RoadNodeWasMigrated"; // BE CAREFUL CHANGING THIS!!

    public required RoadNodeId RoadNodeId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }
    public required bool Grensknoop { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
