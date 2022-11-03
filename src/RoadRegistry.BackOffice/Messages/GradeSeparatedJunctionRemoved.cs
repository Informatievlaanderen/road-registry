namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class GradeSeparatedJunctionRemoved : IMessage
{
    public int Id { get; set; }
}