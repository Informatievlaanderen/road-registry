namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

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

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            // After
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

            message.RoadSegmentAddedToNationalRoad = new Messages.RoadSegmentAddedToNationalRoad
            {
                AttributeId = AttributeId,
                Ident2 = Number,
                SegmentId = SegmentId,
                TemporaryAttributeId = TemporaryAttributeId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Ident2 = Number,
                SegmentId = SegmentId
            };
        }
    }
}
