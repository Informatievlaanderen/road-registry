namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeJunction
{
    public Problems Remove(Provenance provenance)
    {
        var problems = Problems.WithContext(GradeJunctionId);

        if (IsRemoved)
        {
            return problems;
        }

        Apply(new GradeJunctionWasRemoved
        {
            GradeJunctionId = GradeJunctionId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
