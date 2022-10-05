using Be.Vlaanderen.Basisregisters.EventHandling;

namespace RoadRegistry.BackOffice.Messages;

public class GradeSeparatedJunctionRemoved : IMessage
{
    public int Id { get; set; }
}
