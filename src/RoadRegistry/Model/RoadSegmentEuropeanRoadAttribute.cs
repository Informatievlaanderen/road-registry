namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentEuropeanRoadAttribute
    {
        public RoadSegmentEuropeanRoadAttribute(
            AttributeId id,
            AttributeId? temporaryId,
            EuropeanRoadNumber number)
        {
            Id = id;
            TemporaryId = temporaryId;
            Number = number ?? throw new ArgumentNullException(nameof(number));
        }

        public AttributeId Id { get; }
        public AttributeId? TemporaryId { get; }
        public EuropeanRoadNumber Number { get; }
    }
}
