namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Remove(Provenance provenance)
    {
        if (IsRemoved)
        {
            return Problems.None;
        }

        Apply(new GradeSeparatedJunctionWasRemoved
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            Provenance = new ProvenanceData(provenance)
        });

        return Problems.None;
    }
}
