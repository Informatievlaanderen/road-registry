namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using Changes;
using Events;

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
