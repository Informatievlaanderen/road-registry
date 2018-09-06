namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;

    [EventName("RoadNetworkChanged")]
    [EventDescription("Indicates the road network was changed.")]
    public class RoadNetworkChanged
    {
        public RoadNetworkChange[] Changeset { get; set; }
    }
}
