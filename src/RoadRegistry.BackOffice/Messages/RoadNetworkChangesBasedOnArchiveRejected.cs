namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesBasedOnArchiveRejected")]
    [EventDescription("Indicates the road network changes based on an archive were rejected.")]
    public class RoadNetworkChangesBasedOnArchiveRejected
    {
        public string ArchiveId { get; set; }
        public string Reason { get; set; }
        public string Operator { get; set; }
        public string OrganizationId { get; set; }
        public string Organization { get; set; }
        public RejectedChange[] Changes { get; set; }
        public string When { get; set; }
    }
}
