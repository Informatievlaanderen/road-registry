namespace RoadRegistry.Events
{
    using Aiv.Vbr.EventHandling;

    [EventName("ImportedReferencePoint")]
    [EventDescription("Indicates a road network reference point was imported.")]
    public class ImportedReferencePoint
    {
        public int Id { get; set; }
        public byte[] Geometry { get; set; }
        public string Ident8 { get; set; }
        public ReferencePointType Type { get; set; }
        public string Caption { get; set; }
        public OriginProperties Origin { get; set; }
    }
}