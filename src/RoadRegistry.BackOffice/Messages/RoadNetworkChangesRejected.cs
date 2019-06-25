namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesRejected")]
    [EventDescription("Indicates the road network changes were rejected.")]
    public class RoadNetworkChangesRejected
    {
        public RejectedChange[] Changes { get; set; }
        public string When { get; set; }
    }
}
