namespace RoadRegistry.BackOffice.Schema
{
    public class RoadNetworkChangesArchiveAcceptedActivity
    {
        public string ArchiveId { get; set; }
        public RoadNetworkChangesArchiveFile[] Files { get; set; }
    }
}
