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

        // No RoadSegmentStreetNameWasUnlinked first: removing a road segment is letting go of its street names by
        // definition.

        Apply(new RoadSegmentWasRemoved
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
