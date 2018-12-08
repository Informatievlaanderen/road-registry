namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentNationalRoadAttribute
    {
        public RoadSegmentNationalRoadAttribute(
            AttributeId id,
            AttributeId? temporaryId,
            NationalRoadNumber number)
        {
            Id = id;
            TemporaryId = temporaryId;
            Number = number ?? throw new ArgumentNullException(nameof(number));
        }

        public AttributeId Id { get; }
        public AttributeId? TemporaryId { get; }
        public NationalRoadNumber Number { get; }
    }
}
