namespace RoadRegistry.BackOffice.Core
{
    using System;
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

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            // After
            if (!context.View.Segments.TryGetValue(UpperSegmentId, out var upperSegment))
            {
                problems = problems.UpperRoadSegmentMissing();
            }

            // After
            if (!context.View.Segments.TryGetValue(LowerSegmentId, out var lowerSegment))
            {
                problems = problems.LowerRoadSegmentMissing();
            }

            // After
            if (upperSegment != null && lowerSegment != null)
            {
                if (!upperSegment.Geometry.Intersects(lowerSegment.Geometry))
                {
                    problems = problems.UpperAndLowerRoadSegmentDoNotIntersect();
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

            message.GradeSeparatedJunctionAdded = new Messages.GradeSeparatedJunctionAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                UpperRoadSegmentId = UpperSegmentId,
                LowerRoadSegmentId = LowerSegmentId
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
