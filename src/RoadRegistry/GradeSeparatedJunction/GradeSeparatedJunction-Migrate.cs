namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Migrate(MigrateGradeSeparatedJunctionChange change, Provenance provenance)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        Apply(new GradeSeparatedJunctionWasMigrated
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
