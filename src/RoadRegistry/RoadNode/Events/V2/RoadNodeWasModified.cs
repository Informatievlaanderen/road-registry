namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record RoadNodeWasModified : IMartenEvent
{
    public const string EventName = "RoadNodeWasModified"; // BE CAREFUL CHANGING THIS!!

    public required RoadNodeId RoadNodeId { get; init; }
    public RoadNodeGeometry? Geometry { get; init; }
    public bool? Grensknoop { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
