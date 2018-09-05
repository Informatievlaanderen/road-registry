namespace RoadRegistry.Events
{
    public class RoadNodeAdded
    {
        public long Id { get; set;  }
        public RoadNodeType Type { get; set; }
        public byte[] Geometry { get; set; }
    }
}
