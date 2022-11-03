namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class AddRoadSegmentToEuropeanRoad : IMessage
{
    public string Number { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
}