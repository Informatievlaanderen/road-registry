namespace RoadRegistry.BackOffice.Messages
{
    using MessagePack;

    [MessagePackObject]
    public class RoadNetworkSnapshotSegment
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int StartNodeId { get; set; }
        [Key(2)]
        public int EndNodeId { get; set; }
        [Key(3)]
        public RoadSegmentGeometry Geometry { get; set; }
        [Key(4)]
        public RoadNetworkSnapshotSegmentAttributeHash AttributeHash { get; set; }
        [Key(5)]
        public string[] PartOfEuropeanRoads { get; set; }
        [Key(6)]
        public string[] PartOfNationalRoads { get; set; }
        [Key(7)]
        public string[] PartOfNumberedRoads { get; set; }
        [Key(8)]
        public int Version { get; set; }
        [Key(9)]
        public int GeometryVersion { get; set; }
    }
}
