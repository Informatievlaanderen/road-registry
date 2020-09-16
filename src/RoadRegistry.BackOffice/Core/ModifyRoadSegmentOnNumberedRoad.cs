namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class ModifyRoadSegmentOnNumberedRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

        public ModifyRoadSegmentOnNumberedRoad(
            AttributeId attributeId,
            RoadSegmentId segmentId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            AttributeId = attributeId;
            SegmentId = segmentId;
            Number = number;
            Direction = direction;
            Ordinal = ordinal;
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

            message.RoadSegmentAddedToNumberedRoad = new Messages.RoadSegmentAddedToNumberedRoad
            {
                AttributeId = AttributeId,
                Ident8 = Number,
                SegmentId = SegmentId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegmentFromNumberedRoad = new Messages.RemoveRoadSegmentFromNumberedRoad
            {
                AttributeId = AttributeId,
                Ident8 = Number,
                SegmentId = SegmentId
            };
        }
    }
}
