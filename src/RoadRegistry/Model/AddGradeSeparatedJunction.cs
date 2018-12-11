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

        public Messages.AcceptedChange Accept(IReadOnlyCollection<Problem> problems)
        {
            return new Messages.AcceptedChange
            {
                GradeSeparatedJunctionAdded = new Messages.GradeSeparatedJunctionAdded
                {
                    Id = Id,
                    TemporaryId = TemporaryId,
                    Type = Type.ToString(),
                    UpperSegmentId = UpperSegmentId,
                    LowerSegmentId = LowerSegmentId
                },
                Warnings = problems.OfType<Warning>().Select(warning => warning.Translate()).ToArray()
            };
        }

        public Messages.RejectedChange Reject(IReadOnlyCollection<Problem> problems)
        {
            return new Messages.RejectedChange
            {
                AddGradeSeparatedJunction = new Messages.AddGradeSeparatedJunction
                {
                    TemporaryId = TemporaryId,
                    Type = Type.ToString(),
                    UpperSegmentId = TemporaryUpperSegmentId ?? UpperSegmentId,
                    LowerSegmentId = TemporaryLowerSegmentId ?? LowerSegmentId
                },
                Errors = problems.OfType<Error>().Select(error => error.Translate()).ToArray(),
                Warnings = problems.OfType<Warning>().Select(warning => warning.Translate()).ToArray()
            };
        }
    }
}