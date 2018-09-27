namespace RoadRegistry.Model
{
    public class RoadSegmentHardeningAttribute : DynamicRoadSegmentAttribute
    {
        public RoadSegmentHardeningAttribute(
            RoadSegmentHardeningType type,
            RoadSegmentPosition from, 
            RoadSegmentPosition to, 
            GeometryVersion asOfGeometryVersion
        ) : base(from, to, asOfGeometryVersion)
        {
            Type = type;
        }

        public RoadSegmentHardeningType Type { get; }
    }
}