namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RemoveBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        // Removing what is already removed records nothing, the same as Remove() does: a second removal event
        // would leave the projections with a record they have already dropped.
        if (IsRemoved)
        {
            return problems;
        }

        Apply(new RoadSegmentWasRemovedBecauseOfMigration
        {
            RoadSegmentId = RoadSegmentId,
            StartNodeId = StartNodeId,
            EndNodeId = EndNodeId,
            Geometry = Geometry,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
