namespace RoadRegistry.BackOffice.Messages
{
    public class RoadNetworkChangesArchiveAccepted
    {
        public string ArchiveId { get; set; }

        public RequestedChange[] Changes { get; set; }

        public Problem[] Warnings { get; set; }
    }
}
