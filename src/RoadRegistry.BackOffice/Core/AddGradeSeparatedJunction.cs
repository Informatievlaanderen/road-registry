namespace RoadRegistry.BackOffice.Core
{
    using System;
    using Validators;
    using Validators.AddGradeSeparatedJunction.After;

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

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Problems.None;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var validator = new AddGradeSeparatedJunctionWithAfterVerificationContextValidator();
            return validator.Validate((this, context)).ToProblems();
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

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
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

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
