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

        // A removed road node is still loaded with the road network, so a caller that forgets to filter it out can
        // reach this. Nothing can be changed about it any more: the change would be written onto a road node the read
        // models no longer hold.
        if (IsRemoved)
        {
            return problems + new RoadNodeIsRemoved();
        }

        var hasChanges = (change.Geometry is not null && Geometry != change.Geometry)
                         || (change.Grensknoop is not null && Grensknoop != change.Grensknoop);
        if (!hasChanges)
        {
            return problems;
        }

        Apply(new RoadNodeWasChanged
        {
            RoadNodeId = RoadNodeId,
            Geometry = change.Geometry,
            Grensknoop = change.Grensknoop,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
