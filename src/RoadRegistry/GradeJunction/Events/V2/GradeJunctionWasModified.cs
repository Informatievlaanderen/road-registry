namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeJunctionWasModified : IMartenEvent
{
    public required GradeJunctionId GradeJunctionId { get; init; }
    public RoadSegmentId? RoadSegmentId1 { get; init; }
    public RoadSegmentId? RoadSegmentId2 { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
