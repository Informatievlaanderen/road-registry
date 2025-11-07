namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Changes;
using Events;

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
