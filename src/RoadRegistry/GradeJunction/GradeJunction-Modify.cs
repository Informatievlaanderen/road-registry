namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeJunction
{
    // Re-points the grade junction to a different road segment on either side (used when a referenced segment is split
    // and its identifier changes). Only the non-null sides are applied.
    public Problems Modify(RoadSegmentId? roadSegmentId1, RoadSegmentId? roadSegmentId2, Provenance provenance)
    {
        var problems = Problems.WithContext(GradeJunctionId);

        Apply(new GradeJunctionWasModified
        {
            GradeJunctionId = GradeJunctionId,
            RoadSegmentId1 = roadSegmentId1,
            RoadSegmentId2 = roadSegmentId2,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
