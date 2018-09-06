namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;
    using Shared;

    [EventName("ImportedRoadNode")]
    [EventDescription("Indicates a road network node was imported.")]
    public class ImportedRoadNode
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public byte[] Geometry { get; set; }
        public RoadNodeType Type { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
