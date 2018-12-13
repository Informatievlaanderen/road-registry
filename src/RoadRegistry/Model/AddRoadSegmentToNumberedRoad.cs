namespace RoadRegistry.Model
{
    using System;

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

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var errors = Errors.None;

            if (!context.View.Segments.ContainsKey(SegmentId))
            {
                errors = errors.RoadSegmentMissing(TemporarySegmentId ?? SegmentId);
            }

            if (errors.Count > 0)
            {
                return new RejectedChange(this, errors, Warnings.None);
            }
            return new AcceptedChange(this, Warnings.None);
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadSegmentAddedToNumberedRoad = new Messages.RoadSegmentAddedToNumberedRoad
            {
                AttributeId = AttributeId,
                Ident8 = Number,
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
                Ident8 = Number,
                Direction = Direction,
                Ordinal = Ordinal,
                SegmentId = SegmentId
            };
        }
    }
}
