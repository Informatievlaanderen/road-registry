namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesBasedOnArchiveRejectedEntry
    {
        public RoadNetworkChangesArchiveInfo Archive { get; set; }
        public RoadNetworkRejectedChange[] Changes { get; set; }
    }
}