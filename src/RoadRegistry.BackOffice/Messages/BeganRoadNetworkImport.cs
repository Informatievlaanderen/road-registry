namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("BeganRoadNetworkImport")]
[EventDescription("Indicates the import of the road network registry was begun.")]
public class BeganRoadNetworkImport : IMessage, IWhen
{
    public string When { get; set; }
}
