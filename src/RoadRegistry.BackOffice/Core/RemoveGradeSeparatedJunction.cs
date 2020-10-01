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

        public IVerifiedChange Verify(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // TODO: We need a before and after verify because
            // in the before we want to make sure we're dealing with an existing junction

            var problems = Problems.None;

            if (!context.View.GradeSeparatedJunctions.ContainsKey(Id))
            {
                problems = problems.GradeSeparatedJunctionNotFound();
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
