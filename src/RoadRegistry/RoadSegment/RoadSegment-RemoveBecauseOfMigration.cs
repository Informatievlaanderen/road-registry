namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RemoveBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadSegmentId);

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
