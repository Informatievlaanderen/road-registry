namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

public class AddRoadSegmentToNumberedRoad : IMessage
{
    public string Direction { get; set; }
    public string Number { get; set; }
    public int Ordinal { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
}