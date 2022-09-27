namespace RoadRegistry.BackOffice.Messages;

public class AddRoadSegmentToNationalRoad
{
    public int TemporaryAttributeId { get; set; }
    public int SegmentId { get; set; }
    public string Number { get; set; }
}
