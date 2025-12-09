namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Modify(ModifyRoadNodeChange change, Provenance provenance)
    {
        var problems = Problems.None;

        Apply(new RoadNodeModified
        {
            RoadNodeId = RoadNodeId,
            Geometry = change.Geometry?.ToGeometryObject(),
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
