namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.ScopedRoadNetwork;

public partial class GradeJunction
{
    public static GradeJunction Add(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2, Provenance provenance, IRoadNetworkIdGenerator idGenerator)
    {
        var junction = Create(new GradeJunctionWasAdded
        {
            GradeJunctionId = idGenerator.NewGradeJunctionId(),
            RoadSegmentId1 = roadSegmentId1,
            RoadSegmentId2 = roadSegmentId2,
            Provenance = new ProvenanceData(provenance)
        });

        return junction;
    }
}
