namespace RoadRegistry.Projector.Projections.Response;

using System.Collections.Generic;

public class ProjectionsStatusList
{
    public List<ProjectionStatus> Projections { get; set; }
    public long StreamPosition { get; set; }
}

public class ProjectionStatus
{
    public long CurrentPosition { get; set; }
    public string Description { get; set; }
    public string ErrorMessage { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
}
