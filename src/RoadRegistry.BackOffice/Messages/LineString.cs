namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class LineString
{
    [Key(0)] public Point[] Points { get; set; }

    [Key(1)] public double[] Measures { get; set; }
}
