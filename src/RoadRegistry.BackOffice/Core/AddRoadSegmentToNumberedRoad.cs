namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class AddRoadSegmentToNumberedRoad : IRequestedChange
    {
        public AttributeId AttributeId { get; }
        public AttributeId TemporaryAttributeId { get; }
        public RoadSegmentId SegmentId { get; }
        public RoadSegmentId? TemporarySegmentId { get; }
        public NumberedRoadNumber Number { get; }
        public RoadSegmentNumberedRoadDirection Direction { get; }
        public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

        public AddRoadSegmentToNumberedRoad(AttributeId attributeId,
            AttributeId temporaryAttributeId,
            RoadSegmentId segmentId,
            RoadSegmentId? temporarySegmentId,
            NumberedRoadNumber number,
            RoadSegmentNumberedRoadDirection direction,
            RoadSegmentNumberedRoadOrdinal ordinal)
        {
            AttributeId = attributeId;
            TemporaryAttributeId = temporaryAttributeId;
            SegmentId = segmentId;
            TemporarySegmentId = temporarySegmentId;
            Number = number;
            Direction = direction;
            Ordinal = ordinal;
        }

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return Problems.None;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.AfterView.Segments.ContainsKey(SegmentId))
            {
                problems = problems.Add(new RoadSegmentMissing(TemporarySegmentId ?? SegmentId));
            }

            return problems;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentAddedToNumberedRoad = new Messages.RoadSegmentAddedToNumberedRoad
            {
                AttributeId = AttributeId,
                Number = Number,
                Direction = Direction,
                Ordinal = Ordinal,
                SegmentId = SegmentId,
                TemporaryAttributeId = TemporaryAttributeId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
            {
                TemporaryAttributeId = TemporaryAttributeId,
                Number = Number,
                Direction = Direction,
                Ordinal = Ordinal,
                SegmentId = SegmentId
            };
        }
    }
}
