namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using Messages;

public class RemoveRoadNode : IRequestedChange
{
    public RemoveRoadNode(RoadNodeId id)
    {
        Id = id;
    }

    public RoadNodeId Id { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadNodeRemoved = new RoadNodeRemoved
            {
                Id = Id
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadNode = new Messages.RemoveRoadNode
        {
            Id = Id
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        var nodeBefore = context.BeforeView.Nodes[Id];

        foreach (var segment in nodeBefore.Segments)
            if (context.AfterView.View.Segments.TryGetValue(segment, out var foundSegment))
            {
                if (foundSegment.Start == Id) problems = problems.Add(new RoadSegmentStartNodeRefersToRemovedNode(foundSegment.Id, Id));

                if (foundSegment.End == Id) problems = problems.Add(new RoadSegmentEndNodeRefersToRemovedNode(foundSegment.Id, Id));
            }

        return VerifyAfterResult.WithAcceptedChanges(problems, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.Nodes.ContainsKey(Id)) problems = problems.Add(new RoadNodeNotFound());

        return problems;
    }
}
