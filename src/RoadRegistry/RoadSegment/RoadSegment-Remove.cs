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

        // No RoadSegmentStreetNameWasUnlinked first: a removal lets go of the street names by itself (see
        // Apply(RoadSegmentWasRemoved)).

        Apply(new RoadSegmentWasRemoved
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
