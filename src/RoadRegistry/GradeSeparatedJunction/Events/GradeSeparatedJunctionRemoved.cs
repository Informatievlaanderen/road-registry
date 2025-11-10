namespace RoadRegistry.GradeSeparatedJunction.Events;

using BackOffice;

public class GradeSeparatedJunctionRemoved
{
    public required GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
}
