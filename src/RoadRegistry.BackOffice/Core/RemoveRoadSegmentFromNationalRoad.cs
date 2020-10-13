namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RemoveRoadSegmentFromNationalRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public NationalRoadNumber Number { get; }

        public RemoveRoadSegmentFromNationalRoad(
            AttributeId attributeId,
            RoadSegmentId segmentId,
            NationalRoadNumber number)
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
                if (!segment.PartOfNationalRoads.Contains(Number))
                {
                    problems = problems.NationalRoadNumberNotFound(Number);
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

            message.RoadSegmentAddedToNationalRoad = new Messages.RoadSegmentAddedToNationalRoad
            {
                AttributeId = AttributeId,
                Ident2 = Number,
                SegmentId = SegmentId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegmentFromNationalRoad = new Messages.RemoveRoadSegmentFromNationalRoad
            {
                AttributeId = AttributeId,
                Ident2 = Number,
                SegmentId = SegmentId
            };
        }
    }
}
