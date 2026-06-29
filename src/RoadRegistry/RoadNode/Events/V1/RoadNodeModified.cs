namespace RoadRegistry.RoadNode.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class RoadNodeModified : IMartenEvent
{
    public const string EventName = "RoadNodeModified"; // BE CAREFUL CHANGING THIS!!

    public required RoadNodeGeometry Geometry { get; set; }
    public required int RoadNodeId { get; set; }
    public required int Version { get; set; }
    public required string Type { get; set; }

    public required ProvenanceData Provenance { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
