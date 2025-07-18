namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameRenamed")]
[EventDescription("Notifies the domain that a street name has been renamed.")]
public class StreetNameRenamed : IMessage, IWhen
{
    public int StreetNameLocalId { get; set; }
    public int DestinationStreetNameLocalId { get; set; }

    public string When { get; set; }
}
