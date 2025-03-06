namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;

public class RemoveRoadSegments : IRequestedChange
{
    public RemoveRoadSegments(IReadOnlyList<RoadSegmentId> ids, RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        Ids = ids;
        GeometryDrawMethod = geometryDrawMethod;
    }

    public IReadOnlyList<RoadSegmentId> Ids { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        foreach (var id in Ids)
        {
            if (!context.BeforeView.View.Segments.ContainsKey(id))
            {
                problems += new RoadSegmentNotFound(id);
            }
        }

        return problems;
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        // todo-pr: did we create islands?

        // todo-pr: add private variable which noded were changed or deleted

        return problems;
    }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentsRemoved = new RoadSegmentsRemoved
        {
            Ids = Ids.Select(x => x.ToInt32()).ToArray(),
            GeometryDrawMethod = GeometryDrawMethod
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RemoveRoadSegments = new Messages.RemoveRoadSegments
        {
            Ids = Ids.Select(x => x.ToInt32()).ToArray(),
            GeometryDrawMethod = GeometryDrawMethod
        };
    }
}
