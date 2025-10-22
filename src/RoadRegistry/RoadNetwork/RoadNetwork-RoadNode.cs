namespace RoadRegistry.RoadNetwork;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BackOffice;
using RoadNode;

public partial class RoadNetwork
{
    public bool TryGetRoadNode(RoadNodeId roadNodeId, [MaybeNullWhen(false)] out RoadNode node)
    {
        return RoadNodes.TryGetValue(roadNodeId, out node);
    }

    public bool TryGetRoadNode(Predicate<RoadNode> match, [MaybeNullWhen(false)] out RoadNode node)
    {
        node = RoadNodes
            .Where(x => match(x.Value))
            .Select(x => x.Value)
            .FirstOrDefault();
        return node is not null;
    }
}
