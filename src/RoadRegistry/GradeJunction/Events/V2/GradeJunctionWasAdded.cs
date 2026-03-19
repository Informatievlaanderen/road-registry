namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeJunctionWasAdded : IMartenEvent, ICreatedEvent
{
    public required GradeJunctionId GradeJunctionId { get; init; }
    public required RoadSegmentId RoadSegmentId1 { get; init; }
    public required RoadSegmentId RoadSegmentId2 { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
