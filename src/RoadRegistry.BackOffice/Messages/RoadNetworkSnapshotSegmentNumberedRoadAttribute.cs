namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentNumberedRoadAttribute
{
    [Key(0)] public int AttributeId { get; set; }
    [Key(1)] public string Direction { get; set; }
    [Key(2)] public string Number { get; set; }
    [Key(3)] public int Ordinal { get; set; }
}
