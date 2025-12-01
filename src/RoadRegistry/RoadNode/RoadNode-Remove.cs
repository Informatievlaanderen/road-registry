namespace RoadRegistry.RoadNode;

using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Remove()
    {
        var problems = Problems.None;

        Apply(new RoadNodeRemoved
        {
            RoadNodeId = RoadNodeId
        });

        return problems;
    }
}
