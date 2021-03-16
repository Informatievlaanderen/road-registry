namespace RoadRegistry.Editor.Schema
{
    public class RoadNetworkChangesBasedOnArchiveAcceptedEntry
    {
        public RoadNetworkChangesArchiveInfo Archive { get; set; }

        public RoadNetworkAcceptedChange[] Changes { get; set; }

        public RoadNetworkChangesSummary Summary { get; set; }
    }
}
