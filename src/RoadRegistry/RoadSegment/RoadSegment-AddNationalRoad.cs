namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Changes;
using Events;

public partial class RoadSegment
{
    public Problems AddNationalRoad(AddRoadSegmentToNationalRoadChange change)
    {
        if (!Attributes.NationalRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentAddedToNationalRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number
            });
        }

        return Problems.None;
    }
}
