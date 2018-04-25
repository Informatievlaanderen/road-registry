namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;

    [EventName("ImportedRoadNode")]
    [EventDescription("Indicates a road network node was imported.")]
    public class ImportedRoadNode
    {
        public int Id { get; set; }
        public byte[] WellKnownBinaryGeometry { get; set; }
        public RoadNodeType Type { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
