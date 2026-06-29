namespace RoadRegistry.RoadNode.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class RoadNodeAdded : IMartenEvent
{
    public const string EventName = "RoadNodeAdded"; // BE CAREFUL CHANGING THIS!!

    public RoadNodeGeometry Geometry { get; set; }
    public int RoadNodeId { get; set; }
    public int Version { get; set; }
    public int TemporaryId { get; set; }
    public int? OriginalId { get; set; }
    public string Type { get; set; }

    public ProvenanceData Provenance { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
