namespace RoadRegistry.Commands
{
    public class RoadNode
    {
        public int Id { get; set; }
        public byte[] Geometry { get; set; }
        public RoadNodeType Type { get; set; }
        public OriginProperties Origin { get; set; }
    }
}