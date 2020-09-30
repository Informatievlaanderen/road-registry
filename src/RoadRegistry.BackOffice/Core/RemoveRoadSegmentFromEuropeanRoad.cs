namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RemoveRoadSegmentFromEuropeanRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public EuropeanRoadNumber Number { get; }

        public RemoveRoadSegmentFromEuropeanRoad(
            AttributeId attributeId,
            RoadSegmentId segmentId,
            EuropeanRoadNumber number)
        {
            AttributeId = attributeId;
            SegmentId = segmentId;
            Number = number;
        }

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.View.Segments.ContainsKey(SegmentId))
            {
                problems = problems.RoadSegmentMissing(SegmentId);
            }

            if (problems.OfType<Error>().Any())
            {
                return new RejectedChange(this, problems);
            }
            return new AcceptedChange(this, problems);
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentRemovedFromEuropeanRoad = new Messages.RoadSegmentRemovedFromEuropeanRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId,
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegmentFromEuropeanRoad = new Messages.RemoveRoadSegmentFromEuropeanRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId
            };
        }
    }
}
