namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RebuildRoadNetworkSnapshot")]
    [EventDescription("Indicates that a rebuild of the road network snapshot got requested")]
    public class RebuildRoadNetworkSnapshot
    {
        public int StartFromVersion { get; set; }
    }
}
