namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RetireBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        if (IsRemoved)
        {
            return problems;
        }

        Apply(new RoadSegmentWasRetiredBecauseOfMigration
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
