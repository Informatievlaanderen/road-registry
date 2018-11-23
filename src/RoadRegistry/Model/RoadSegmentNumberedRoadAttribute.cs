namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentNumberedRoadAttribute
    {
        public RoadSegmentNumberedRoadAttribute(
            AttributeId id,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            Id = id;
            Number = number;
            Direction = direction ?? throw new ArgumentNullException(nameof(direction));
            Ordinal = ordinal;
        }

        public AttributeId Id { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }
    }
}
