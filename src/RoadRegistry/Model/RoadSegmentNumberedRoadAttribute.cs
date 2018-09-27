namespace RoadRegistry.Model
{
    using System;
    
    public class RoadSegmentNumberedRoadAttribute
    {
        public RoadSegmentNumberedRoadAttribute(
            NumberedRoadNumber number, 
            RoadSegmentNumberedRoadDirection direction, 
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            Number = number;
            Direction = direction ?? throw new ArgumentNullException(nameof(direction));
            Ordinal = ordinal;
        }

        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }
    }
}
