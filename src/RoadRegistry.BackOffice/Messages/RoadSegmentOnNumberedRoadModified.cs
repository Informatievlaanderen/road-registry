namespace RoadRegistry.BackOffice.Messages;

public class RoadSegmentOnNumberedRoadModified
{
    public int AttributeId { get; set; }
    public string Direction { get; set; }
    public string Number { get; set; }
    public int Ordinal { get; set; }
    public int SegmentId { get; set; }
}