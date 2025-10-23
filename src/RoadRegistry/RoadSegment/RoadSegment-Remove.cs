namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Events;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class RoadSegment
{
    public Problems Remove(RemoveRoadSegmentChange change, RoadNetworkChangeContext context)
    {
        ApplyChange(new RoadSegmentRemoved
        {
            Id = change.Id
        });

        return Problems.None;
    }
}
