namespace RoadRegistry.Commands
{
    public class AddRoadNode
    {
        public long Id { get; set; }
        public Events.RoadNodeType Type { get; set; }
        public byte[] Geometry { get; set; }
    }
}
