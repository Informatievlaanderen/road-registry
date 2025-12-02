namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Remove(Provenance provenance)
    {
        Apply(new GradeSeparatedJunctionRemoved
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            Provenance = new ProvenanceData(provenance)
        });

        return Problems.None;
    }
}
