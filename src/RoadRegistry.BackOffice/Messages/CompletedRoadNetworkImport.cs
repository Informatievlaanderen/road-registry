namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("CompletedRoadNetworkImport")]
[EventDescription("Indicates the import of the legacy road network was finished.")]
public class CompletedRoadNetworkImport
{
    public string When { get; set; }
}