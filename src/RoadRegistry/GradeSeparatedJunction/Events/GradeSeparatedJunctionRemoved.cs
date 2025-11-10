namespace RoadRegistry.GradeSeparatedJunction.Events;

using BackOffice;

public record GradeSeparatedJunctionRemoved
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
