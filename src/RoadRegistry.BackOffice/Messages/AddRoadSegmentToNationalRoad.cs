namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegmentToNationalRoad
{
    public string Number { get; set; }
    public int SegmentId { get; set; }
    public int TemporaryAttributeId { get; set; }
}
