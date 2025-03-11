namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;

public class RemoveRoadSegments : IRequestedChange
{
    private readonly RoadNetworkVersionProvider _roadNetworkVersionProvider;

    public RemoveRoadSegments(
        IReadOnlyList<RoadSegmentId> ids,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadNetworkVersionProvider roadNetworkVersionProvider)
    {
        _roadNetworkVersionProvider = roadNetworkVersionProvider;
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

    private readonly List<RoadNodeId> _removedRoadNodeIds = [];
    private readonly List<RoadNodeTypeChanged> _changedRoadNodes = [];
    private readonly List<GradeSeparatedJunctionId> _removedJunctionIds = [];

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        // todo-pr: did we create islands?

        var relatedRoadNodes = new List<RoadNodeId>();
        foreach (var id in Ids)
        {
            context.BeforeView.View.Segments.TryGetValue(id, out var segment);
            relatedRoadNodes.Add(segment!.Start);
            relatedRoadNodes.Add(segment!.End);
        }
        relatedRoadNodes = relatedRoadNodes.Distinct().ToList();

        _removedRoadNodeIds.AddRange(relatedRoadNodes.Where(x => !context.AfterView.Nodes.ContainsKey(x)));

        foreach (var roadNodeId in relatedRoadNodes.Except(_removedRoadNodeIds))
        {
            context.BeforeView.Nodes.TryGetValue(roadNodeId, out var roadNodeBefore);
            context.AfterView.Nodes.TryGetValue(roadNodeId, out var roadNodeAfter);

            if (roadNodeBefore!.Type == roadNodeAfter!.Type)
            {
                continue;
            }

            _changedRoadNodes.Add(new RoadNodeTypeChanged
            {
                Id = roadNodeId,
                Type = roadNodeAfter.Type,
                Version = _roadNetworkVersionProvider.NextRoadNodeVersion(roadNodeId)
            });
        }

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
