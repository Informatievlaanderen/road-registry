namespace RoadRegistry.BackOffice.Messages
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesBasedOnArchiveAccepted")]
    [EventDescription("Indicates the road network changes based on an archive were accepted.")]
    public class RoadNetworkChangesBasedOnArchiveAccepted
    {
        public String ArchiveId { get; set; }
        public AcceptedChange[] Changes { get; set; }
        public string When { get; set; }
    }
}
