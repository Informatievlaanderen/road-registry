namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveRejectedEntry
    {
        public RoadNetworkChangesArchiveInfo Archive { get; set; }
        public RoadNetworkChangesArchiveFile[] Files { get; set; }
    }
}
