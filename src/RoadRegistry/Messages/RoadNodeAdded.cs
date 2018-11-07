namespace RoadRegistry.Messages
{
    public class RoadNodeAdded
    {
        public int Id { get; set;  }
        public RoadNodeType Type { get; set; }
        public byte[] Geometry { get; set; }
    }
}
