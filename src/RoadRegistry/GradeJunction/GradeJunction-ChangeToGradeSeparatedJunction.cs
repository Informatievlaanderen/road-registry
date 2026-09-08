namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeJunction
{
    // Hands the crossing over to a grade separated junction: this one stops existing, and says which junction took
    // its place.
    public Problems ChangeToGradeSeparatedJunction(GradeSeparatedJunctionId gradeSeparatedJunctionId, Provenance provenance)
    {
        var problems = Problems.WithContext(GradeJunctionId);

        if (IsRemoved)
        {
            return problems;
        }

        Apply(new GradeJunctionWasChangedToGradeSeparatedJunction
        {
            GradeJunctionId = GradeJunctionId,
            GradeSeparatedJunctionId = gradeSeparatedJunctionId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
