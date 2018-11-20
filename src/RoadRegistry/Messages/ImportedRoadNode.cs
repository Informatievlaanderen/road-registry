namespace RoadRegistry.Messages
{
    using System;
    using Aiv.Vbr.EventHandling;

    [EventName("ImportedRoadNode")]
    [EventDescription("Indicates a road network node was imported.")]
    public class ImportedRoadNode
    {
        public int Id { get; set; }
        public int Version { get; set; }
        [Obsolete("Please use Geometry2 instead.")]
        public byte[] Geometry { get; set; }
        public RoadNodeGeometry Geometry2 { get; set; }
        public string Type { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
