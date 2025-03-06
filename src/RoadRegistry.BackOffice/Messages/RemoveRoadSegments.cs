namespace RoadRegistry.BackOffice.Messages;

public class RemoveRoadSegments
{
    public int[] Ids { get; set; }
    public string GeometryDrawMethod { get; set; }
}
