namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;
using ValueObjects.Problems;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public class RemoveGradeSeparatedJunction : IRequestedChange
{
    public RemoveGradeSeparatedJunction(GradeSeparatedJunctionId id)
    {
        Id = id;
    }

    public GradeSeparatedJunctionId Id { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(Problem[] warnings)
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

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveGradeSeparatedJunction = new Messages.RemoveGradeSeparatedJunction
        {
            Id = Id
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return VerifyAfterResult.WithAcceptedChanges(Problems.None, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.GradeSeparatedJunctions.ContainsKey(Id)) problems = problems.Add(new GradeSeparatedJunctionNotFound());

        return problems;
    }
}
