namespace RoadRegistry.BackOffice.Messages
{
    using Aiv.Vbr.EventHandling;

    [EventName("CompletedRoadNetworkImport")]
    [EventDescription("Indicates the import of the legacy road network was finished.")]
    public class CompletedRoadNetworkImport
    {}
}
