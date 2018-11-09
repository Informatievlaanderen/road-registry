namespace RoadRegistry.Messages
{
    using Aiv.Vbr.EventHandling;

    [EventName("ImportedRoadNode")]
    [EventDescription("Indicates a road network node was imported.")]
    public class ImportedRoadNode
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public byte[] Geometry { get; set; }
        public string Type { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
