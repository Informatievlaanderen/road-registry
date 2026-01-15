namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using ValueObjects.Problems;

public partial class RoadNode
{
    public static (RoadNode?, Problems) Migrate(MigrateRoadNodeChange change, Provenance provenance)
    {
        var problems = Problems.For(change.RoadNodeId);

        var node = Create(new RoadNodeWasMigrated
        {
            RoadNodeId = change.RoadNodeId,
            Geometry = change.Geometry,
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return (node, problems);
    }
}
