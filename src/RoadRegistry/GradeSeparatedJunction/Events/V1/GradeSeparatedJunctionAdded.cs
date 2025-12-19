namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class GradeSeparatedJunctionAdded : IMartenEvent
{
    public required int Id { get; set; }
    public required int LowerRoadSegmentId { get; set; }
    public required int TemporaryId { get; set; }
    public required string Type { get; set; }
    public required int UpperRoadSegmentId { get; set; }

    public required ProvenanceData Provenance { get; init; }
}
