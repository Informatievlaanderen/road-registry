namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameModified")]
[EventDescription("Notifies the domain that a street name has been modified.")]
public class StreetNameModified : IMessage
{
    //TODO-rik StreetNameModified

    public string When { get; set; }
}
