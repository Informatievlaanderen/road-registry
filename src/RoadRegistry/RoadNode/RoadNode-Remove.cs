namespace RoadRegistry.RoadNode;

using BackOffice.Core;
using Events;

public partial class RoadNode
{
    public Problems Remove()
    {
        var problems = Problems.None;

        Apply(new RoadNodeRemoved
        {
            Id = RoadNodeId
        });

        return problems;
    }
}
