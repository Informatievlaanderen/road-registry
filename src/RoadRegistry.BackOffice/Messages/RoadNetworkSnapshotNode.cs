namespace RoadRegistry.BackOffice.Messages
{
    using MessagePack;

    [MessagePackObject]
    public class RoadNetworkSnapshotNode
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public int[] Segments { get; set; }
        [Key(2)]
        public RoadNodeGeometry Geometry { get; set; }
    }
}
