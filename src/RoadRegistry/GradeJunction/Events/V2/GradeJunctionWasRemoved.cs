namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeJunctionWasRemoved : IMartenEvent
{
    public required GradeJunctionId GradeJunctionId { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
