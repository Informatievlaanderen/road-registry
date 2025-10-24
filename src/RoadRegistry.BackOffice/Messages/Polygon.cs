namespace RoadRegistry.BackOffice.Messages;

public class Polygon
{
    public Ring[] Holes { get; set; }
    public Ring Shell { get; set; }
}