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

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: We need a before and after verify because
            // in the before we want to make sure we're dealing with an existing node

            var problems = Problems.None;

            if (!context.View.Nodes.ContainsKey(Id))
            {
                problems = problems.RoadNodeNotFound();
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
