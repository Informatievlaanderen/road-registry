namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeJunction
{
    // public Problems Modify(ModifyGradeJunctionChange change, Provenance provenance)
    // {
    //     var problems = Problems.WithContext(GradeJunctionId);
    //
    //     Apply(new GradeJunctionWasModified
    //     {
    //         GradeJunctionId = GradeJunctionId,
    //         RoadSegmentId1 = change.RoadSegmentId1,
    //         RoadSegmentId2 = change.RoadSegmentId2,
    //         Type = change.Type,
    //         Provenance = new ProvenanceData(provenance)
    //     });
    //
    //     return problems;
    // }
}
