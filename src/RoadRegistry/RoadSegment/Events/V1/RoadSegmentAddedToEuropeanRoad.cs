namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class RoadSegmentAddedToEuropeanRoad : IMartenEvent
{
    public required int AttributeId { get; set; }
    public required string Number { get; set; }
    public required int SegmentId { get; set; }
    public required int TemporaryAttributeId { get; set; }
    public required int? SegmentVersion { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
