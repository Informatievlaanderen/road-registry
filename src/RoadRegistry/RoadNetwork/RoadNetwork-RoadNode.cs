namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using RoadNode.Changes;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using RoadNode = RoadNode.RoadNode;

public partial class RoadNetwork
{
    public IEnumerable<RoadNode> GetNonRemovedRoadNodes()
    {
        return _roadNodes.Values.Where(x => !x.IsRemoved);
    }

    private Problems AddRoadNode(RoadNetworkChanges changes, AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        var (roadNode, problems) = RoadNode.Add(change, changes.Provenance, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        problems += idTranslator.RegisterMapping(change.TemporaryId, roadNode!.RoadNodeId);
        if (problems.HasError())
        {
            return problems;
        }

        _roadNodes.Add(roadNode.RoadNodeId, roadNode);
        summary.Added.Add(roadNode.RoadNodeId);

        return problems;
    }

    private Problems ModifyRoadNode(RoadNetworkChanges changes, ModifyRoadNodeChange change, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var problems = roadNode.Modify(change, changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Modified.Add(roadNode.RoadNodeId);
        return problems;
    }

    private Problems RemoveRoadNode(RoadNetworkChanges changes, RemoveRoadNodeChange change, RoadNetworkEntityChangesSummary<RoadNodeId> summary)
    {
        if (!_roadNodes.TryGetValue(change.RoadNodeId, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.RoadNodeId));
        }

        var problems = roadNode.Remove(changes.Provenance);
        if (problems.HasError())
        {
            return problems;
        }

        summary.Removed.Add(roadNode.RoadNodeId);
        return problems;
    }
}
