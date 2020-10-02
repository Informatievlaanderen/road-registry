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

            // Before
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

            if (problems.OfType<Error>().Any())
            {
                return new RejectedChange(this, problems);
            }
            return new AcceptedChange(this, problems);
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentOnNumberedRoadModified = new Messages.RoadSegmentOnNumberedRoadModified
            {
                AttributeId = AttributeId,
                Ident8 = Number,
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
                Ident8 = Number,
                SegmentId = SegmentId,
                Direction = Direction,
                Ordinal = Ordinal
            };
        }
    }
}
