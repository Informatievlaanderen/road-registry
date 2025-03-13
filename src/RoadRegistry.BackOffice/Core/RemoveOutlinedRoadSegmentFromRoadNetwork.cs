namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;

public class RemoveOutlinedRoadSegmentFromRoadNetwork : IRequestedChange
{
    public RemoveOutlinedRoadSegmentFromRoadNetwork(RoadSegmentId id)
    {
        Id = id;
    }

    public RoadSegmentId Id { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadSegmentRemoved = new RoadSegmentRemoved
            {
                Id = Id,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined
            }
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RemoveOutlinedRoadSegmentFromRoadNetwork = new Messages.RemoveOutlinedRoadSegmentFromRoadNetwork
        {
            Id = Id
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return Problems.None;
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
