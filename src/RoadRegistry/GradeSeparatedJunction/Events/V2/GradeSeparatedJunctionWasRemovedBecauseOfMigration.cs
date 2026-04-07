namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeSeparatedJunctionWasRemovedBecauseOfMigration : IMartenEvent
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId LowerRoadSegmentId { get; init; }
    public RoadSegmentId UpperRoadSegmentId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
