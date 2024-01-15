namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("StreetNameModified")]
[EventDescription("Notifies the domain that a street name has been modified.")]
public class StreetNameModified : IMessage
{
    public StreetNameRecord Record { get; set; }

    public bool NameChanged { get; set; }
    public bool HomonymAdditionChanged { get; set; }
    public bool StatusChanged { get; set; }
    public bool Restored { get; set; }

    public string When { get; set; }
}
