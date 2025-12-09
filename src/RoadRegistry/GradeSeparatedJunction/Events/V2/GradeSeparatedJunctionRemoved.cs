namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeSeparatedJunctionRemoved : IMartenEvent
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public GradeSeparatedJunctionRemoved()
    {
    }
    protected GradeSeparatedJunctionRemoved(GradeSeparatedJunctionRemoved other) // Needed for Marten
    {
    }
}
