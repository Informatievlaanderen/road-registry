namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameCreated")]
[EventDescription("Notifies the domain that a street name has been created.")]
public class StreetNameCreated : IMessage
{
    //TODO-rik StreetNameCreated

    public string When { get; set; }
}
