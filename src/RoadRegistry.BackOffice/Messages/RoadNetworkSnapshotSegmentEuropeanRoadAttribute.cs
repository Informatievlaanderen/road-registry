namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentEuropeanRoadAttribute
{
    [Key(0)] public int AttributeId { get; set; }
    [Key(1)] public string Number { get; set; }
}
