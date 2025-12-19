namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Remove(Provenance provenance)
    {
        var problems = Problems.For(RoadNodeId);

        if (IsRemoved)
        {
            return problems;
        }

        Apply(new RoadNodeWasRemoved
        {
            RoadNodeId = RoadNodeId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
