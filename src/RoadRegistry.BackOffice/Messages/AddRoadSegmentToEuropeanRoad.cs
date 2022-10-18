namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegmentToEuropeanRoad
{
    public string Number { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
}