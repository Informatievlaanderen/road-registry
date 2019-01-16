namespace RoadRegistry.Messages
{
    using Aiv.Vbr.EventHandling;

    [EventName("RoadNetworkChangesRejected")]
    [EventDescription("Indicates the road network changes were rejected.")]
    public class RoadNetworkChangesRejected
    {
        public RejectedChange[] Changes { get; set; }
    }
}
