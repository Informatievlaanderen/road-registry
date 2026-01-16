namespace RoadRegistry.Projector.Projections;

public class ProjectionStatus
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required long CurrentPosition { get; set; }
    public required long StorePosition { get; set; }
    public required string State { get; set; }
    public required string ErrorMessage { get; set; }
}
