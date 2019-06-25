namespace RoadRegistry.BackOffice.Messages
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkChangesAccepted")]
    [EventDescription("Indicates the road network changes were accepted.")]
    public class RoadNetworkChangesAccepted
    {
        public AcceptedChange[] Changes { get; set; }
        public string When { get; set; }
    }
}
