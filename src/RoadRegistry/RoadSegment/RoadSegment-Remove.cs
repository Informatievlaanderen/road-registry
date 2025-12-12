namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems Remove(Provenance provenance)
    {
        if (IsRemoved)
        {
            return Problems.None;
        }

        Apply(new RoadSegmentWasRemoved
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return Problems.None;
    }
}
