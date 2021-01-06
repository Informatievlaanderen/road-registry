namespace RoadRegistry.BackOffice.Core
{
    using System;

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

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.BeforeView.View.Segments.ContainsKey(SegmentId))
            {
                problems = problems.Add(new RoadSegmentMissing(SegmentId));
            }

            return problems;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            var segment = context.AfterView.View.Segments[SegmentId];
            if (!segment.PartOfNumberedRoads.Contains(Number))
            {
                problems = problems.Add(new NumberedRoadNumberNotFound(Number));
            }

            return problems;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentOnNumberedRoadModified = new Messages.RoadSegmentOnNumberedRoadModified
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId,
                Direction = Direction,
                Ordinal = Ordinal
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.ModifyRoadSegmentOnNumberedRoad = new Messages.ModifyRoadSegmentOnNumberedRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                SegmentId = SegmentId,
                Direction = Direction,
                Ordinal = Ordinal
            };
        }
    }
}
