namespace RoadRegistry.Projector.Projections;

public class ProjectionStatus
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public long CurrentPosition { get; set; }
    public string State { get; set; }
    public string ErrorMessage { get; set; }
}
