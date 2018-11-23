namespace RoadRegistry.Model
{
    using System;

    public class RoadSegmentEuropeanRoadAttribute
    {
        public RoadSegmentEuropeanRoadAttribute(
            AttributeId id,
            EuropeanRoadNumber number)
        {
            Id = id;
            Number = number ?? throw new ArgumentNullException(nameof(number));
        }

        public AttributeId Id { get; }
        public EuropeanRoadNumber Number { get; }
    }
}
