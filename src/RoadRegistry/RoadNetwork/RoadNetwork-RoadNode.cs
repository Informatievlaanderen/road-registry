namespace RoadRegistry.RoadNetwork;

using BackOffice.Core;
using Changes;
using ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public partial class RoadNetwork
{
    private Problems AddRoadNode(AddRoadNodeChange change, RoadNetworkChangeContext context)
    {
        var (roadNode, problems) = RoadNode.Add(change, context);
        if (problems.HasError())
        {
            return problems;
        }

        _identifierTranslator.RegisterMapping(change.TemporaryId, roadNode!.RoadNodeId);
        _roadNodes.Add(roadNode.RoadNodeId, roadNode);
        return problems;
    }

    // private Problems ModifyRoadNode(ModifyRoadNodeChange change, RoadNetworkChangeContext context)
    // {
    //     if (!_roadNodes.TryGetValue(change.Id, out var roadNode))
    //     {
    //         return Problems.Single(new RoadNodeNotFound(change.OriginalId ?? change.Id));
    //     }
    //
    //     return roadNode.Modify(change, context);
    // }
    //
    // private Problems RemoveRoadSegment(RemoveRoadNodeChange change, RoadNetworkChangeContext context)
    // {
    //     if (!_roadNodes.TryGetValue(change.Id, out var roadNode))
    //     {
    //         return Problems.Single(new RoadNodeNotFound(change.Id));
    //     }
    //
    //     return roadNode.Remove(change, context);
    // }
}
