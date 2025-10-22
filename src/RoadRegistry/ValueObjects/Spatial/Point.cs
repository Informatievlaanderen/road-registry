namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class Point
{
    [Key(0)] public double X { get; set; }
    [Key(1)] public double Y { get; set; }
}