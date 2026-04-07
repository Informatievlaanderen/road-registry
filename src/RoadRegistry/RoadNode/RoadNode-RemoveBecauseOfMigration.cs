namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems RemoveBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadNodeId);

        Apply(new RoadNodeWasRemovedBecauseOfMigration
        {
            RoadNodeId = RoadNodeId,
            Geometry = Geometry,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
