namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveRejectedActivity
    {
        public string ArchiveId { get; set; }
        public RoadNetworkChangesArchiveFile[] Files { get; set; }
    }
}
