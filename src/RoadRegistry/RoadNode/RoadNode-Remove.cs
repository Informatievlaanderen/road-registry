namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Remove(Provenance provenance)
    {
        if (IsRemoved)
        {
            return Problems.None;
        }

        Apply(new RoadNodeWasRemoved
        {
            RoadNodeId = RoadNodeId,
            Provenance = new ProvenanceData(provenance)
        });

        return Problems.None;
    }
}
