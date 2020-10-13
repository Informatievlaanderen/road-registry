namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RemoveRoadSegmentFromNumberedRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NumberedRoadNumber Number { get; }

        public RemoveRoadSegmentFromNumberedRoad(
            AttributeId attributeId,
            RoadSegmentId segmentId,
            NumberedRoadNumber number)
        {
            AttributeId = attributeId;
            SegmentId = segmentId;
            Number = number;
        }

        public Problems VerifyBefore(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.View.Segments.TryGetValue(SegmentId, out var segment))
            {
                problems = problems.RoadSegmentMissing(SegmentId);
            }
            else
            {
                if (!segment.PartOfNumberedRoads.Contains(Number))
                {
                    problems = problems.NumberedRoadNumberNotFound(Number);
                }
            }

            return problems;
        }

        public Problems VerifyAfter(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Problems.None;
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
