namespace RoadRegistry.Projections
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;

    public class RoadShapeRunner : Runner<ShapeContext>
    {
        public RoadShapeRunner(EnvelopeFactory envelopeFactory) :
            base(
                "RoadOsloRunner",
                envelopeFactory,
                new RoadNodeRecordProjection())
            { }
    }
}
