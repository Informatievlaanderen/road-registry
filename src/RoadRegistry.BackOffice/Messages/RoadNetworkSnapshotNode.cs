namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotNode
{
    [Key(2)] public RoadNodeGeometry Geometry { get; set; }
    [Key(0)] public int Id { get; set; }
    [Key(1)] public int[] Segments { get; set; }
    [Key(3)] public string Type { get; set; }
}