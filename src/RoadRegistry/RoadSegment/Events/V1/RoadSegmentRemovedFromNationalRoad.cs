namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class RoadSegmentRemovedFromNationalRoad : IMartenEvent
{
    public const string EventName = "RoadSegmentRemovedFromNationalRoad"; // BE CAREFUL CHANGING THIS!!

    public required int AttributeId { get; set; }
    public required string Number { get; set; }
    public required int RoadSegmentId { get; set; }
    public required int? RoadSegmentVersion { get; set; }

    public required ProvenanceData Provenance { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
