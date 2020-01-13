namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesBasedOnArchiveAccepted")]
    [EventDescription("Indicates the road network changes based on an archive were accepted.")]
    public class RoadNetworkChangesBasedOnArchiveAccepted
    {
        public string ArchiveId { get; set; }
        public string Reason { get; set; }
        public string Operator { get; set; }
        public string Organization { get; set; }
        public AcceptedChange[] Changes { get; set; }
        public string When { get; set; }
    }
}
