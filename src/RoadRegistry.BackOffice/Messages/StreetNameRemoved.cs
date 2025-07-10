namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameRemoved")]
[EventDescription("Notifies the domain that a street name has been removed.")]
public class StreetNameRemoved : IMessage, IWhen
{
    public string StreetNameId { get; set; }

    public string When { get; set; }
}
