namespace RoadRegistry.Projections
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using NetTopologySuite;
    using NetTopologySuite.IO;

    public class RoadShapeRunner : Runner<ShapeContext>
    {
        public RoadShapeRunner(EnvelopeFactory envelopeFactory) :
            base(
                "RoadShapeRunner",
                envelopeFactory,
                new RoadNodeRecordProjection(
                    new WKBReader(new NtsGeometryServices()),
                    new RoadNodeTypeTranslator()
                ))
            { }
    }
}
