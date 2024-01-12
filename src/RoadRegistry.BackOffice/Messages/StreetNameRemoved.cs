namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameRemoved")]
[EventDescription("Notifies the domain that a street name has been removed.")]
public class StreetNameRemoved : IMessage
{
    //TODO-rik StreetNameRemoved

    public string When { get; set; }
}
