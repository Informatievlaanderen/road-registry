namespace RoadRegistry.RoadNode;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using ValueObjects.Problems;

public partial class RoadNode
{
    public Problems Modify(ModifyRoadNodeChange change, Provenance provenance)
    {
        var problems = Problems.WithContext(RoadNodeId);

        var hasChanges = (change.Geometry is not null && Geometry != change.Geometry)
                         || (change.Grensknoop is not null && Grensknoop != change.Grensknoop);
        if (!hasChanges)
        {
            return problems;
        }

        Apply(new RoadNodeWasModified
        {
            RoadNodeId = RoadNodeId,
            Geometry = change.Geometry,
            Grensknoop = change.Grensknoop,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
