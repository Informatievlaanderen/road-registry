namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveRejectedEntry
    {
        public string ArchiveId { get; set; }
        public RoadNetworkChangesArchiveFile[] Files { get; set; }
    }
}
