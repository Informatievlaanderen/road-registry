namespace RoadRegistry.GradeSeparatedJunction.Events;

public record GradeSeparatedJunctionRemoved
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
