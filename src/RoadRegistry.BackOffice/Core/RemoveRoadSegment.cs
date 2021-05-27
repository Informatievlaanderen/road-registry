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

            if (!context.BeforeView.View.Segments.ContainsKey(Id))
            {
                problems = problems.Add(new RoadSegmentNotFound());
            }

            return problems;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            var segmentBefore = context.BeforeView.Segments[Id];

            if (context.AfterView.View.Nodes.TryGetValue(segmentBefore.Start, out var beforeStartNode))
            {
                problems = problems.AddRange(
                    beforeStartNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            }

            if (context.AfterView.View.Nodes.TryGetValue(segmentBefore.End, out var beforeEndNode))
            {
                problems = problems.AddRange(
                    beforeEndNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            }

            return problems;
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
