namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class RoadSegmentAddedToNumberedRoad : IMartenEvent
{
    public required int AttributeId { get; set; }
    public required string Direction { get; set; }
    public required string Number { get; set; }
    public required int Ordinal { get; set; }
    public required int RoadSegmentId { get; set; }
    public required int TemporaryAttributeId { get; set; }
    public required int? RoadSegmentVersion { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
