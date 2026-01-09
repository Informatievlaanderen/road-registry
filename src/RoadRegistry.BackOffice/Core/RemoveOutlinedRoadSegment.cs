namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;
using Problem = RoadRegistry.Infrastructure.Messages.Problem;

public class RemoveOutlinedRoadSegment : IRequestedChange
{
    public RemoveOutlinedRoadSegment(RoadSegmentId id)
    {
        Id = id;
    }

    public RoadSegmentId Id { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            OutlinedRoadSegmentRemoved = new OutlinedRoadSegmentRemoved
            {
                Id = Id
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RemoveOutlinedRoadSegment = new Messages.RemoveOutlinedRoadSegment
        {
            Id = Id
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return VerifyAfterResult.WithAcceptedChanges(Problems.None, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        if (!context.BeforeView.View.Segments.ContainsKey(Id))
        {
            problems = problems.Add(new RoadSegmentNotFound(Id));
        }

        return problems;
    }
}
