namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentNumberedRoadAttribute
    {
        public RoadSegmentNumberedRoadAttribute(
            AttributeId id,
            AttributeId? temporaryId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            Id = id;
            TemporaryId = temporaryId;
            Number = number;
            Direction = direction ?? throw new ArgumentNullException(nameof(direction));
            Ordinal = ordinal;
        }

        public AttributeId Id { get; }
        public AttributeId? TemporaryId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }
    }
}
