namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using Extensions;
using ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Modify(ModifyRoadNodeChange change, Provenance provenance)
    {
        var problems = Problems.For(RoadNodeId);

        Apply(new RoadNodeWasModified
        {
            RoadNodeId = RoadNodeId,
            Geometry = change.Geometry?.ToRoadNodeGeometry(),
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
