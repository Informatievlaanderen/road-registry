namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesArchiveRejected")]
    [EventDescription("Indicates the road network changes archive was rejected.")]
    public class RoadNetworkChangesArchiveRejected
    {
        public string ArchiveId { get; set; }
        public Problem[] Errors { get; set; }
        public Problem[] Warnings { get; set; }
    }
}
