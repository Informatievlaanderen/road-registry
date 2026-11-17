namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems RemoveBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        // Removing what is already removed records nothing, the same as Remove() does: a second removal event
        // would leave the projections with a record they have already dropped.
        if (IsRemoved)
        {
            return problems;
        }

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
