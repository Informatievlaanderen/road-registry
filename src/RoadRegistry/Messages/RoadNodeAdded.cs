namespace RoadRegistry.Messages
{
    public class RoadNodeAdded
    {
        public int Id { get; set;  }
        public string Type { get; set; }
        public byte[] Geometry { get; set; }
    }
}
