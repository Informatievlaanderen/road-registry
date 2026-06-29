namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record RoadNodeWasRemovedBecauseOfMigration : IMartenEvent
{
    public const string EventName = "RoadNodeWasRemovedBecauseOfMigration"; // BE CAREFUL CHANGING THIS!!

    public required RoadNodeId RoadNodeId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
