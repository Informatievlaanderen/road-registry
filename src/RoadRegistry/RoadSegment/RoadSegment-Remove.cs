namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Events;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems Remove(RemoveRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        var originalStartRoadNode = context.RoadNetwork.RoadNodes[StartNodeId];
        var originalEndRoadNode = context.RoadNetwork.RoadNodes[EndNodeId];

        Apply(new RoadSegmentRemoved
        {
            Id = change.Id
        });

        originalStartRoadNode.DisconnectFrom(RoadSegmentId);
        originalEndRoadNode.DisconnectFrom(RoadSegmentId);

        return Problems.None;
    }
}
