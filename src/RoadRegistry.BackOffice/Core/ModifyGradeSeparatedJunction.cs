namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class ModifyGradeSeparatedJunction : IRequestedChange
    {
        public ModifyGradeSeparatedJunction(
            GradeSeparatedJunctionId id,
            GradeSeparatedJunctionType type,
            RoadSegmentId upperSegmentId,
            RoadSegmentId? temporaryUpperSegmentId,
            RoadSegmentId lowerSegmentId,
            RoadSegmentId? temporaryLowerSegmentId)
        {
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            UpperSegmentId = upperSegmentId;
            TemporaryUpperSegmentId = temporaryUpperSegmentId;
            LowerSegmentId = lowerSegmentId;
            TemporaryLowerSegmentId = temporaryLowerSegmentId;
        }

        public GradeSeparatedJunctionId Id { get; }
        public GradeSeparatedJunctionType Type { get; }
        public RoadSegmentId UpperSegmentId { get; }
        public RoadSegmentId? TemporaryUpperSegmentId { get; }
        public RoadSegmentId LowerSegmentId { get; }
        public RoadSegmentId? TemporaryLowerSegmentId { get; }

        public Problems VerifyBefore(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var problems = Problems.None;

            if (!context.View.GradeSeparatedJunctions.ContainsKey(Id))
            {
                problems = problems.GradeSeparatedJunctionNotFound();
            }

            return problems;
        }

        public Problems VerifyAfter(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.View.Segments.TryGetValue(UpperSegmentId, out var upperSegment))
            {
                problems = problems.UpperRoadSegmentMissing();
            }

            if (!context.View.Segments.TryGetValue(LowerSegmentId, out var lowerSegment))
            {
                problems = problems.LowerRoadSegmentMissing();
            }

            if (upperSegment != null && lowerSegment != null)
            {
                if (!upperSegment.Geometry.Intersects(lowerSegment.Geometry))
                {
                    problems = problems.UpperAndLowerRoadSegmentDoNotIntersect();
                }
            }

            return problems;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.GradeSeparatedJunctionModified = new Messages.GradeSeparatedJunctionModified
            {
                Id = Id,
                Type = Type.ToString(),
                UpperRoadSegmentId = UpperSegmentId,
                LowerRoadSegmentId = LowerSegmentId
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.ModifyGradeSeparatedJunction = new Messages.ModifyGradeSeparatedJunction
            {
                Id = Id,
                Type = Type.ToString(),
                UpperSegmentId = TemporaryUpperSegmentId ?? UpperSegmentId,
                LowerSegmentId = TemporaryLowerSegmentId ?? LowerSegmentId
            };
        }
    }
}
