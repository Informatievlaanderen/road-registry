namespace RoadRegistry.GradeSeparatedJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public record GradeSeparatedJunctionModified : IMartenEvent
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId? LowerRoadSegmentId { get; init; }
    public RoadSegmentId? UpperRoadSegmentId { get; init; }
    public GradeSeparatedJunctionType? Type { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public GradeSeparatedJunctionModified()
    {
    }
    protected GradeSeparatedJunctionModified(GradeSeparatedJunctionModified other) // Needed for Marten
    {
    }
}
