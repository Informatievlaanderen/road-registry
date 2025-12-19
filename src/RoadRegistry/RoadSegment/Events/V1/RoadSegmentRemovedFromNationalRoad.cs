namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public class RoadSegmentRemovedFromNationalRoad : IMartenEvent
{
    public required int AttributeId { get; set; }
    public required string Number { get; set; }
    public required int RoadSegmentId { get; set; }
    public required int? RoadSegmentVersion { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
