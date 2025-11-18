namespace RoadRegistry.RoadNetwork;

using System.Collections.Generic;
using System.Linq;
using BackOffice.Core;
using RoadNode.Changes;
using RoadNode = RoadNode.RoadNode;

public partial class RoadNetwork
{
    public IEnumerable<RoadNode> GetNonRemovedRoadNodes()
    {
        return _roadNodes.Values.Where(x => !x.IsRemoved);
    }

    private Problems AddRoadNode(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var (roadNode, problems) = RoadNode.Add(change, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        idTranslator.RegisterMapping(change.TemporaryId, roadNode!.RoadNodeId);
        _roadNodes.Add(roadNode.RoadNodeId, roadNode);
        return problems;
    }

    private Problems ModifyRoadNode(ModifyRoadNodeChange change)
    {
        if (!_roadNodes.TryGetValue(change.Id, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.Id));
        }

        return roadNode.Modify(change);
    }

    private Problems RemoveRoadNode(RemoveRoadNodeChange change)
    {
        if (!_roadNodes.TryGetValue(change.Id, out var roadNode))
        {
            return Problems.Single(new RoadNodeNotFound(change.Id));
        }

        return roadNode.Remove();
    }
}
