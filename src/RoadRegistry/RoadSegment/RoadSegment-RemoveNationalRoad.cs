namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RemoveNationalRoad(RemoveRoadSegmentFromNationalRoadChange change, Provenance provenance)
    {
        if (Attributes.NationalRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentRemovedFromNationalRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number,
                Provenance = new ProvenanceData(provenance)
            });
        }

        return Problems.None;
    }
}
