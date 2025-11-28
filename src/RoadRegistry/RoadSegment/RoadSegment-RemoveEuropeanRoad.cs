namespace RoadRegistry.RoadSegment;

using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

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
