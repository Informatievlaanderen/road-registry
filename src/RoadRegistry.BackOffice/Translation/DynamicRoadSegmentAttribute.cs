namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;

    public abstract class DynamicRoadSegmentAttribute
    {
        protected DynamicRoadSegmentAttribute(
            AttributeId temporaryId,
            RoadSegmentPosition from,
            RoadSegmentPosition to
        )
        {
            if(from >= to)
            {
                throw new ArgumentException(nameof(From),
                $"The from position ({from.ToDecimal()}) must be less than the to position ({to.ToDecimal()}).");
            }

            TemporaryId = temporaryId;
            From = from;
            To = to;
        }

        public AttributeId TemporaryId { get; }

        public RoadSegmentPosition From { get; }

        public RoadSegmentPosition To { get; }
    }
}
