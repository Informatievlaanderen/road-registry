namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Events;

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
