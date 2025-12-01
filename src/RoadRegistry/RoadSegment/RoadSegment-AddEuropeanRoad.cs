namespace RoadRegistry.RoadSegment;

using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems AddEuropeanRoad(AddRoadSegmentToEuropeanRoadChange change)
    {
        if (!Attributes.EuropeanRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentAddedToEuropeanRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number
            });
        }

        return Problems.None;
    }
}
