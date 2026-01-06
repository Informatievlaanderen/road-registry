namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Migrate(ModifyRoadNodeChange change, Provenance provenance)
    {
        var problems = Problems.For(RoadNodeId);

        Apply(new RoadNodeWasMigrated
        {
            RoadNodeId = RoadNodeId,
            Geometry = (change.Geometry ?? Geometry).ToGeometryObject(),
            Type = change.Type ?? Type,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
