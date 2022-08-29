namespace RoadRegistry.Wfs.Projections
{
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("SynchronizeWithStreetNameCache")]
    [EventDescription("Internal event to synchronize the wfs projection with the street name cache.")]
    public class SynchronizeWithStreetNameCache
    {
        public int BatchSize { get; set; }
    }
}
