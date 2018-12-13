namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AddGradeSeparatedJunction : IRequestedChange
    {
        public AddGradeSeparatedJunction(
            GradeSeparatedJunctionId id,
            GradeSeparatedJunctionId temporaryId,
            GradeSeparatedJunctionType type,
            RoadSegmentId upperSegmentId,
            RoadSegmentId? temporaryUpperSegmentId,
            RoadSegmentId lowerSegmentId,
            RoadSegmentId? temporaryLowerSegmentId)
        {
            Id = id;
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            UpperSegmentId = upperSegmentId;
            TemporaryUpperSegmentId = temporaryUpperSegmentId;
            LowerSegmentId = lowerSegmentId;
            TemporaryLowerSegmentId = temporaryLowerSegmentId;
        }

        public GradeSeparatedJunctionId Id { get; }
        public GradeSeparatedJunctionId TemporaryId { get; }
        public GradeSeparatedJunctionType Type { get; }
        public RoadSegmentId UpperSegmentId { get; }
        public RoadSegmentId? TemporaryUpperSegmentId { get; }
        public RoadSegmentId LowerSegmentId { get; }
        public RoadSegmentId? TemporaryLowerSegmentId { get; }

        public IVerifiedChange Verify(ChangeContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var errors = Errors.None;
            if (!context.View.Segments.TryGetValue(UpperSegmentId, out var upperSegment))
            {
                errors = errors.UpperRoadSegmentMissing();
            }

            if (!context.View.Segments.TryGetValue(LowerSegmentId, out var lowerSegment))
            {
                errors = errors.LowerRoadSegmentMissing();
            }

            if (upperSegment != null && lowerSegment != null)
            {
                if (!upperSegment.Geometry.Intersects(lowerSegment.Geometry))
                {
                    errors = errors.UpperAndLowerRoadSegmentDoNotIntersect();
                }
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

            message.GradeSeparatedJunctionAdded = new Messages.GradeSeparatedJunctionAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                UpperSegmentId = UpperSegmentId,
                LowerSegmentId = LowerSegmentId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
            {
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                UpperSegmentId = TemporaryUpperSegmentId ?? UpperSegmentId,
                LowerSegmentId = TemporaryLowerSegmentId ?? LowerSegmentId
            };
        }
    }
}
