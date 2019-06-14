namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveAcceptedEntry
    {
        public string ArchiveId { get; set; }
        public RoadNetworkChangesArchiveFile[] Files { get; set; }
    }
}
