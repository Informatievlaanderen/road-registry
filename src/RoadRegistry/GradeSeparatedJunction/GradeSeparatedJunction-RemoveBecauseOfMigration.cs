namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems RemoveBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        Apply(new GradeSeparatedJunctionWasRemovedBecauseOfMigration
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            LowerRoadSegmentId = LowerRoadSegmentId,
            UpperRoadSegmentId = UpperRoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
