namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class AddRoadSegmentToEuropeanRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public EuropeanRoadNumber Number { get; }

        public AddRoadSegmentToEuropeanRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            EuropeanRoadNumber number)
        {
            AttributeId = attributeId;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            TemporarySegmentId = temporarySegmentId;
            Number = number;
        }

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.View.Segments.ContainsKey(SegmentId))
            {
                problems = problems.RoadSegmentMissing(TemporarySegmentId ?? SegmentId);
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

            message.RoadSegmentAddedToEuropeanRoad = new Messages.RoadSegmentAddedToEuropeanRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId,
                TemporaryAttributeId = TemporaryAttributeId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Number = Number,
                SegmentId = SegmentId
            };
        }
    }
}
