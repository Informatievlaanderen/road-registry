namespace RoadRegistry.RoadNetwork;

using BackOffice.Core;
using RoadNode.Changes;
using ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public partial class RoadNetwork
{
    private Problems AddRoadNode(AddRoadNodeChange change, IRoadNetworkIdGenerator idGenerator)
    {
        var (roadNode, problems) = RoadNode.Add(change, idGenerator);
        if (problems.HasError())
        {
            return problems;
        }

        _identifierTranslator.RegisterMapping(change.TemporaryId, roadNode!.RoadNodeId);
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
