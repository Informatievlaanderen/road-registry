namespace RoadRegistry.BackOffice.Messages;

public class RoadSegmentAddedToEuropeanRoad
{
    public int AttributeId { get; set; }
    public int TemporaryAttributeId { get; set; }
    public string Number { get; set; }
    public int SegmentId { get; set; }
}
