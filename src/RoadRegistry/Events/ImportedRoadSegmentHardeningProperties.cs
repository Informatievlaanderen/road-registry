namespace RoadRegistry.Events
{
    using Shared;

    public class ImportedRoadSegmentHardeningProperties
    {
        public int AttributeId { get; set; }
        public HardeningType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
        public OriginProperties Origin { get; set; }
    }
}
