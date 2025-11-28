namespace RoadRegistry.RoadSegment;

using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems Remove()
    {
        Apply(new RoadSegmentRemoved
        {
            RoadSegmentId = RoadSegmentId
        });

        return Problems.None;
    }
}
