namespace RoadRegistry.Model
{
    public class AddRoadSegmentToNumberedRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

        public AddRoadSegmentToNumberedRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            AttributeId = attributeId;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            TemporarySegmentId = temporarySegmentId;
            Number = number;
            Direction = direction;
            Ordinal = ordinal;
        }
    }
}
