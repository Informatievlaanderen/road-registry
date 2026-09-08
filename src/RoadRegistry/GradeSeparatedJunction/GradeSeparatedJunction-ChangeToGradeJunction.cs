namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    // Hands the crossing over to a grade junction: this one stops existing, and says which junction took its place.
    public Problems ChangeToGradeJunction(GradeJunctionId gradeJunctionId, Provenance provenance)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        if (IsRemoved)
        {
            return problems;
        }

        Apply(new GradeSeparatedJunctionWasChangedToGradeJunction
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            GradeJunctionId = gradeJunctionId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
