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

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.BeforeView.Segments.TryGetValue(SegmentId, out var segment))
            {
                problems = problems.Add(new RoadSegmentMissing(SegmentId));
            }
            else
            {
                if (!segment.PartOfNationalRoads.Contains(Number))
                {
                    problems = problems.Add(new NationalRoadNumberNotFound(Number));
                }
            }

            return problems;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Problems.None;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentRemovedFromNationalRoad = new Messages.RoadSegmentRemovedFromNationalRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegmentFromNationalRoad = new Messages.RemoveRoadSegmentFromNationalRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId
            };
        }
    }
}
