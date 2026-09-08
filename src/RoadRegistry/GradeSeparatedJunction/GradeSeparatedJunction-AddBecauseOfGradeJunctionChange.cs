namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ScopedRoadNetwork;

public partial class GradeSeparatedJunction
{
    // The crossing a grade junction used to record, recorded as a grade separated one. The geometry is the grade
    // junction's: it is the same crossing point, only its kind changed.
    public static GradeSeparatedJunction AddBecauseOfGradeJunctionChange(
        GradeJunctionId gradeJunctionId,
        RoadSegmentId lowerRoadSegmentId,
        RoadSegmentId upperRoadSegmentId,
        GradeSeparatedJunctionTypeV2 type,
        JunctionGeometry geometry,
        Provenance provenance,
        IRoadNetworkIdGenerator idGenerator,
        IEventOrdinalProvider? ordinalProvider = null)
    {
        return CreateWithProvider(new GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange
        {
            GradeSeparatedJunctionId = idGenerator.NewGradeSeparatedJunctionId(),
            GradeJunctionId = gradeJunctionId,
            LowerRoadSegmentId = lowerRoadSegmentId,
            UpperRoadSegmentId = upperRoadSegmentId,
            Type = type,
            Geometry = geometry,
            Provenance = new ProvenanceData(provenance)
        }, ordinalProvider ?? EventOrdinalProvider.None);
    }
}
