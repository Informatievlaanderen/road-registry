namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentNationalRoadAttribute
{
    [Key(0)] public int AttributeId { get; set; }
    [Key(1)] public string Number { get; set; }
}
