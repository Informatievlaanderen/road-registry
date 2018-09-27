namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;

    [EventName("RoadNetworkChangesetRejected")]
    [EventDescription("Indicates the road network changes were rejected.")]
    public class RoadNetworkChangesRejected
    {
        public RejectedChange[] Rejections { get; set; }
    }

}
