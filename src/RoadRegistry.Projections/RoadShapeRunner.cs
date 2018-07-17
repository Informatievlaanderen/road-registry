namespace RoadRegistry.Projections
{
    using System.Text;
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite.IO;

    public class RoadShapeRunner : Runner<ShapeContext>
    {
        public RoadShapeRunner(EnvelopeFactory envelopeFactory, ILoggerFactory loggerFactory, WKBReader reader) :
            base(
                "RoadShapeRunner",
                envelopeFactory,
                loggerFactory.CreateLogger("RoadShapeRunner"),
                new RoadNodeRecordProjection(
                    reader,
                    new RoadNodeTypeTranslator(),
                    Encoding.GetEncoding(1252)
                ),
                new RoadSegmentRecordProjection(
                    reader,
                    new RoadSegmentStatusTranslator(),
                    new RoadSegmentMorphologyTranslator(),
                    new RoadSegmentCategoryTranslator(),
                    new RoadSegmentGeometryDrawMethodTranslator(),
                    new RoadSegmentAccessRestrictionTranslator(),
                    Encoding.GetEncoding(1252)
                ),
                new RoadReferencePointRecordProjection(
                    reader,
                    new ReferencePointTypeTranslator(),
                    Encoding.GetEncoding(1252)
                ),
                new RoadSegmentDynamicHardeningAttributeRecordProjection(
                    new HardeningTypeTranslator(),
                    Encoding.GetEncoding(1252)
                ),
                new RoadSegmentDynamicLaneAttributeRecordProjection(
                    new LaneDirectionTranslator(),
                    Encoding.GetEncoding(1252)
                ),
                new RoadSegmentDynamicWidthAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentEuropeanRoadAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentNationalRoadAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentNumberedRoadAttributeRecordProjection(
                    new NumberedRoadSegmentDirectionTranslator(),
                    Encoding.GetEncoding(1252)
                ),
                new OrganizationRecordProjection(Encoding.GetEncoding(1252)),
                new GradeSeparatedJunctionRecordProjection(
                    new GradeSeparatedJunctionTypeTranslator(),
                    Encoding.GetEncoding(1252)
                ))
            { }
    }
}
