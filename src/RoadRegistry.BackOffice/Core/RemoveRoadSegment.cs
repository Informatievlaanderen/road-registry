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

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.BeforeView.Segments.ContainsKey(Id))
            {
                problems = problems.Add(new RoadSegmentNotFound());
            }

            return problems;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Problems.None;
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
