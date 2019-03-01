namespace RoadRegistry.BackOffice.Messages
{
    public class RoadNetworkChangesArchiveRejected
    {
        public string ArchiveId { get; set; }
        public Problem[] Errors { get; set; }
        public Problem[] Warnings { get; set; }
    }
}
