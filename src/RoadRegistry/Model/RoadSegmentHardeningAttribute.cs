namespace RoadRegistry.Model
{
    public class RoadSegmentHardeningAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentHardeningAttribute(
            HardeningType type,
            Position from, 
            Position to, 
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Type = type;
        }

        public HardeningType Type { get; }
    }
}