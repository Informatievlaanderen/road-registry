namespace RoadRegistry.GradeSeparatedJunction.Events;

public record GradeSeparatedJunctionRemoved
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }

    public GradeSeparatedJunctionRemoved()
    {
    }
    protected GradeSeparatedJunctionRemoved(GradeSeparatedJunctionRemoved other) // Needed for Marten
    {
    }
}
