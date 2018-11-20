namespace RoadRegistry.Messages
{
    using System;

    public class AddRoadNode
    {
        public int Id { get; set; }
        public string Type { get; set; }
        [Obsolete("Please use Geometry2 instead.")]
        public byte[] Geometry { get; set; }
        public RoadNodeGeometry Geometry2 { get; set; }
    }
}
