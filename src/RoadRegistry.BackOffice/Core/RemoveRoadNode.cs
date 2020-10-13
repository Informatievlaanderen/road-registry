namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RemoveRoadNode : IRequestedChange
    {
        public RemoveRoadNode(RoadNodeId id)
        {
            Id = id;
        }

        public RoadNodeId Id { get; }

        public Problems VerifyBefore(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.View.Nodes.ContainsKey(Id))
            {
                problems = problems.Add(new RoadNodeNotFound());
            }

            return problems;
        }

        public Problems VerifyAfter(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return Problems.None;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadNodeRemoved = new Messages.RoadNodeRemoved
            {
                Id = Id
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveRoadNode = new Messages.RemoveRoadNode
            {
                Id = Id
            };
        }
    }
}
