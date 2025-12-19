namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork.ValueObjects;

public class RequestedRoadSegmentSurfaceAttribute
{
    public int AttributeId { get; set; }
    public decimal FromPosition { get; set; }
    public decimal ToPosition { get; set; }
    public string Type { get; set; }
}