namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Remove(Provenance provenance)
    {
        var problems = Problems.None;

        Apply(new RoadNodeRemoved
        {
            RoadNodeId = RoadNodeId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
