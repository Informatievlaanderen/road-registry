namespace RoadRegistry.GradeJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ScopedRoadNetwork;

public partial class GradeJunction
{
    // The crossing a grade separated junction used to record, recorded as a grade one. Which road becomes 'wegsegment
    // 1' and which 'wegsegment 2' does not matter - a grade junction says nothing about which road passes over the
    // other - so the lower and upper road segment are taken as they come.
    public static GradeJunction AddBecauseOfGradeSeparatedJunctionChange(
        GradeSeparatedJunctionId gradeSeparatedJunctionId,
        RoadSegmentId roadSegmentId1,
        RoadSegmentId roadSegmentId2,
        JunctionGeometry geometry,
        Provenance provenance,
        IRoadNetworkIdGenerator idGenerator,
        IEventOrdinalProvider? ordinalProvider = null)
    {
        return CreateWithProvider(new GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange
        {
            GradeJunctionId = idGenerator.NewGradeJunctionId(),
            GradeSeparatedJunctionId = gradeSeparatedJunctionId,
            RoadSegmentId1 = roadSegmentId1,
            RoadSegmentId2 = roadSegmentId2,
            Geometry = geometry,
            Provenance = new ProvenanceData(provenance)
        }, ordinalProvider ?? EventOrdinalProvider.None);
    }
}
