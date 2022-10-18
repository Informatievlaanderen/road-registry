namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class LineString
{
    [Key(1)] public double[] Measures { get; set; }
    [Key(0)] public Point[] Points { get; set; }
}