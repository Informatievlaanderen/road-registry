namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;

    public class RemoveGradeSeparatedJunction : IRequestedChange
    {
        public RemoveGradeSeparatedJunction(GradeSeparatedJunctionId id)
        {
            Id = id;
        }

        public GradeSeparatedJunctionId Id { get; }

        public Problems VerifyBefore(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var problems = Problems.None;

            if (!context.View.GradeSeparatedJunctions.ContainsKey(Id))
            {
                problems = problems.Add(new GradeSeparatedJunctionNotFound());
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

            message.GradeSeparatedJunctionRemoved = new Messages.GradeSeparatedJunctionRemoved
            {
                Id = Id
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RemoveGradeSeparatedJunction = new Messages.RemoveGradeSeparatedJunction
            {
                Id = Id
            };
        }
    }
}
