namespace RoadRegistry.Projector.Projections;

using System.Collections.Generic;

public class ProjectionsStatusList
{
    public List<ProjectionStatus> Projections { get; set; }
    public long StreamPosition { get; set; }
}
