namespace RoadRegistry.GradeSeparatedJunction.Events;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

//TODO-pr dit is enkel een V2 event
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
