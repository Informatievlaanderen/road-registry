namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RemoveRoadSegment : IRequestedChange
    {
        public RemoveRoadSegment(RoadSegmentId id)
        {
            Id = id;
        }

        public RoadSegmentId Id { get; }

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: We need a before and after verify because
            // in the before we want to make sure we're dealing with an existing segment

            var problems = Problems.None;

            if (!context.View.Segments.ContainsKey(Id))
            {
                problems = problems.RoadSegmentNotFound();
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

            message.RoadSegmentRemoved = new Messages.RoadSegmentRemoved
            {
                Id = Id
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadSegment = new Messages.RemoveRoadSegment
            {
                Id = Id
            };
        }
    }
}
