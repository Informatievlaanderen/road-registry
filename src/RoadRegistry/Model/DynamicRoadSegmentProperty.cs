namespace RoadRegistry.Model
{
    public class DynamicRoadSegmentProperty<TProperty>
    {
        public DynamicRoadSegmentProperty(
            TProperty property,
            Position from, 
            Position to, 
            GeometryVersion asOfVersion)
        {
            
        }
    }

    public class RoadSegmentLaneAttribute
    {
    }

    public class RoadSegmentWidthAttribute
    {}

    public class RoadSegmentHardeningAttribute
    {
    }
}