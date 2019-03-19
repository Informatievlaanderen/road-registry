namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesArchiveAccepted")]
    [EventDescription("Indicates the road network changes archive was accepted.")]
    public class RoadNetworkChangesArchiveAccepted
    {
        public string ArchiveId { get; set; }

        public Problem[] Warnings { get; set; }
    }
}
