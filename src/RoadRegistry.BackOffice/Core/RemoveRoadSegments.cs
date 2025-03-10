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

    private List<RoadNodeId> _removedRoadNodeIds = [];
    private List<RoadNodeTypeChanged> _changedRoadNodes = [];
    private List<GradeSeparatedJunctionId> _removedJunctionIds = [];

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        // todo-pr: did we create islands?

        // todo-pr: add private variable which noded were changed or deleted
        var nodes = new List<RoadNodeId>();
        foreach (var id in Ids)
        {
            context.BeforeView.View.Segments.TryGetValue(id, out var segment);
            nodes.Add(segment!.Start);
            nodes.Add(segment!.End);
        }

        nodes = nodes.Distinct().ToList();

        _removedRoadNodeIds.AddRange(nodes.Where(x => !context.AfterView.Nodes.ContainsKey(x)));

        return problems;
    }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadSegmentsRemoved = new RoadSegmentsRemoved
        {
            GeometryDrawMethod = GeometryDrawMethod,
            RemovedRoadSegmentIds = Ids.Select(x => x.ToInt32()).ToArray(),
            RemovedRoadNodeIds = _removedRoadNodeIds.Select(x => x.ToInt32()).ToArray(),
            ChangedRoadNodes = _changedRoadNodes.ToArray(),
            RemovedGradeSeparatedJunctionIds = _removedJunctionIds.Select(x => x.ToInt32()).ToArray(),
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
