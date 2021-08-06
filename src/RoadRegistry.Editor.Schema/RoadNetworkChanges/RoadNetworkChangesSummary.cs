namespace RoadRegistry.Editor.Schema.RoadNetworkChanges
{
    public class RoadNetworkChangesSummary
    {
        public RoadNetworkChangeCounters RoadNodes { get; set; }
        public RoadNetworkChangeCounters RoadSegments { get; set; }
        public RoadNetworkChangeCounters GradeSeparatedJunctions { get; set; }
    }
}
