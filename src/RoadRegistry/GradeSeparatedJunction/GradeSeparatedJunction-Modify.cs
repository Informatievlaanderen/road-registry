namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Modify(ModifyGradeSeparatedJunctionChange change, Provenance provenance)
    {
        var problems = Problems.None;

        Apply(new GradeSeparatedJunctionModified
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
