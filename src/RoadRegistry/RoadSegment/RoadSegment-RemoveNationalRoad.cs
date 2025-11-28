namespace RoadRegistry.RoadSegment;

using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RemoveNationalRoad(RemoveRoadSegmentFromNationalRoadChange change)
    {
        if (Attributes.NationalRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentRemovedFromNationalRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number
            });
        }

        return Problems.None;
    }
}
