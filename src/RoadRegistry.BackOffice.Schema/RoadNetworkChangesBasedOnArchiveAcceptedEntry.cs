namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesBasedOnArchiveAcceptedEntry
    {
        public RoadNetworkChangesArchiveInfo Archive { get; set; }

        public RoadNetworkAcceptedChange[] Changes { get; set; }
    }
}
