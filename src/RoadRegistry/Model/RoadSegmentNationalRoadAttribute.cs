namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentNationalRoadAttribute
    {
        public RoadSegmentNationalRoadAttribute(
            AttributeId id,
            NationalRoadNumber number)
        {
            Id = id;
            Number = number ?? throw new ArgumentNullException(nameof(number));
        }

        public AttributeId Id { get; }
        public NationalRoadNumber Number { get; }
    }
}
