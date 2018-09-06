namespace RoadRegistry.Commands
{
    using Shared;

    public class AddRoadNode
    {
        public long Id { get; set; }
        public RoadNodeType Type { get; set; }
        public byte[] Geometry { get; set; }
    }
}
