namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveAcceptedEntry
    {
        public RoadNetworkChangesArchiveInfo Archive { get; set; }
        public RoadNetworkChangesArchiveFile[] Files { get; set; }
    }
}
