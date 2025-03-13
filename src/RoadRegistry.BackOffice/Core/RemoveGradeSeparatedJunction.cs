namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;

public class RemoveGradeSeparatedJunction : IRequestedChange
{
    public RemoveGradeSeparatedJunction(GradeSeparatedJunctionId id)
    {
        Id = id;
    }

    public GradeSeparatedJunctionId Id { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            GradeSeparatedJunctionRemoved = new GradeSeparatedJunctionRemoved
            {
                Id = Id
            }
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

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return Problems.None;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.GradeSeparatedJunctions.ContainsKey(Id)) problems = problems.Add(new GradeSeparatedJunctionNotFound());

        return problems;
    }
}
