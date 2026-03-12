namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;

public partial class GradeJunction
{
    // public static (GradeJunction?, Problems) Add(AddGradeJunctionChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator)
    // {
    //     var problems = Problems.WithContext(change.TemporaryId);
    //
    //     var junction = Create(new GradeJunctionWasAdded
    //     {
    //         GradeJunctionId = idGenerator.NewGradeJunctionId(),
    //         OriginalId = change.TemporaryId,
    //         RoadSegmentId1 = change.RoadSegmentId1,
    //         RoadSegmentId2 = change.RoadSegmentId2,
    //         Type = change.Type,
    //         Provenance = new ProvenanceData(provenance)
    //     });
    //
    //     return (junction, problems);
    // }
}
