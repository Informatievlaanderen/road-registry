namespace RoadRegistry.BackOffice.Messages;

using MessagePack;

[MessagePackObject]
public class RoadNetworkSnapshotSegmentAttributeHash
{
    [Key(0)] public string AccessRestriction { get; set; }
    [Key(1)] public string Category { get; set; }
    [Key(2)] public string Morphology { get; set; }
    [Key(3)] public string Status { get; set; }
    [Key(4)] public int? LeftSideStreetNameId { get; set; }
    [Key(5)] public int? RightSideStreetNameId { get; set; }
    [Key(6)] public string OrganizationId { get; set; }
    [Key(7)] public string GeometryDrawMethod { get; set; }
}
