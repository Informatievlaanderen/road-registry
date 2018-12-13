namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
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

        public IVerifiedChange Verify(ChangeContext context)
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

            message.RoadSegmentAddedToEuropeanRoad = new Messages.RoadSegmentAddedToEuropeanRoad
            {
                AttributeId = AttributeId,
                RoadNumber = Number,
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
                RoadNumber = Number,
                SegmentId = SegmentId
            };
        }
    }
}
