namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Changes;
using Events;

public partial class RoadSegment
{
    public Problems RemoveEuropeanRoad(RemoveRoadSegmentFromEuropeanRoadChange change)
    {
        if (Attributes.EuropeanRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentRemovedFromEuropeanRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number
            });
        }

        return Problems.None;
    }
}
