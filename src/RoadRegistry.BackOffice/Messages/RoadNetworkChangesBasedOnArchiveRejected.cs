namespace RoadRegistry.BackOffice.Messages
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesBasedOnArchiveRejected")]
    [EventDescription("Indicates the road network changes based on an archive were rejected.")]
    public class RoadNetworkChangesBasedOnArchiveRejected
    {
        public String ArchiveId { get; set; }
        public RejectedChange[] Changes { get; set; }
        public string When { get; set; }
    }
}
