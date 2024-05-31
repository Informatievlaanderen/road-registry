namespace RoadRegistry.Integration.Schema.RoadNetworkChanges;

public class RoadNetworkChangesSummary
{
    public RoadNetworkChangeCounters GradeSeparatedJunctions { get; set; }
    public RoadNetworkChangeCounters RoadNodes { get; set; }
    public RoadNetworkChangeCounters RoadSegments { get; set; }
}