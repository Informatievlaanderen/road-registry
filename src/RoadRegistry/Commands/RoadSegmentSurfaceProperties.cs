namespace RoadRegistry.Commands
{
    public class RoadSegmentSurfaceProperties
    {
        public int AttributeId { get; set; }
        public SurfaceType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public OriginProperties Origin { get; set; }
    }
}