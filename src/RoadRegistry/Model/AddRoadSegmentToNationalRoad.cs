namespace RoadRegistry.Model
{
    public class AddRoadSegmentToNationalRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public NationalRoadNumber Number { get; }

        public AddRoadSegmentToNationalRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            NationalRoadNumber number)
        {
            AttributeId = attributeId;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            TemporarySegmentId = temporarySegmentId;
            Number = number;
        }
    }
}
