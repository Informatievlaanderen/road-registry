namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems AddEuropeanRoad(AddRoadSegmentToEuropeanRoadChange change, Provenance provenance)
    {
        if (!Attributes.EuropeanRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentAddedToEuropeanRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number,
                Provenance = new ProvenanceData(provenance)
            });
        }

        return Problems.None;
    }
}
