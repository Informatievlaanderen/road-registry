namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems Remove(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        if (IsRemoved)
        {
            return problems;
        }

        // The street names are let go before the removal rather than after it: the projections drop a removed road
        // segment, so a change recorded after the removal would land on a record they no longer hold.
        UnlinkStreetNames(provenance);

        Apply(new RoadSegmentWasRemoved
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
