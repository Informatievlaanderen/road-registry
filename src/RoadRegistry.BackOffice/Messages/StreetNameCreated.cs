namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameCreated")]
[EventDescription("Notifies the domain that a street name has been created.")]
public class StreetNameCreated : IMessage
{
    public StreetNameRecord Record { get; set; }

    public string When { get; set; }
}
