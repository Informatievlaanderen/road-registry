namespace RoadRegistry.Messages
{
    using Aiv.Vbr.EventHandling;

    [EventName("RoadNetworkChangesAccepted")]
    [EventDescription("Indicates the road network changes were accepted.")]
    public class RoadNetworkChangesAccepted
    {
        public AcceptedChange[] Changes { get; set; }
    }
}
