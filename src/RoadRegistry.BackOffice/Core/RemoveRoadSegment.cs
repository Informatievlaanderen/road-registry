namespace RoadRegistry.BackOffice.Core;

using System;
using Messages;

public class RemoveRoadSegment : IRequestedChange
{
    public RemoveRoadSegment(RoadSegmentId id, RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        Id = id;
        GeometryDrawMethod = geometryDrawMethod;
    }

    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentRemoved = new RoadSegmentRemoved
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegment = new Messages.RemoveRoadSegment
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        var segmentBefore = context.BeforeView.Segments[Id];

        if (context.AfterView.View.Nodes.TryGetValue(segmentBefore.Start, out var beforeStartNode))
            problems = problems.AddRange(
                beforeStartNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

        if (context.AfterView.View.Nodes.TryGetValue(segmentBefore.End, out var beforeEndNode))
            problems = problems.AddRange(
                beforeEndNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

        return problems;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        if (!context.BeforeView.View.Segments.ContainsKey(Id)) problems = problems.Add(new RoadSegmentNotFound(Id));

        return problems;
    }
}
