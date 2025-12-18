namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RemoveEuropeanRoad(RemoveRoadSegmentFromEuropeanRoadChange change, Provenance provenance)
    {
        var problems = Problems.For(RoadSegmentId);

        if (Attributes.EuropeanRoadNumbers.Contains(change.Number))
        {
            Apply(new RoadSegmentWasRemovedFromEuropeanRoad
            {
                RoadSegmentId = change.RoadSegmentId,
                Number = change.Number,
                Provenance = new ProvenanceData(provenance)
            });
        }

        return problems;
    }
}
