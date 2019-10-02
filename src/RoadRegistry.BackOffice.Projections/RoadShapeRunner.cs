namespace RoadRegistry.BackOffice.Projections
{
    using System.Text;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Microsoft.Extensions.Logging;
    using Schema;

    public class RoadShapeRunner : Runner<ShapeContext>
    {
        public RoadShapeRunner(EnvelopeFactory envelopeFactory, ILoggerFactory loggerFactory, WellKnownBinaryReader reader) :
            base(
                "RoadShapeRunner",
                envelopeFactory,
                loggerFactory.CreateLogger("RoadShapeRunner"),
                new RoadNodeRecordProjection(reader,Encoding.GetEncoding(1252)),
                new RoadSegmentRecordProjection(reader,Encoding.GetEncoding(1252)),
                new RoadSegmentSurfaceAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentLaneAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentWidthAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentEuropeanRoadAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentNationalRoadAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new RoadSegmentNumberedRoadAttributeRecordProjection(Encoding.GetEncoding(1252)),
                new OrganizationRecordProjection(Encoding.GetEncoding(1252)),
                new GradeSeparatedJunctionRecordProjection(Encoding.GetEncoding(1252)),
                new RoadNetworkInfoProjection(reader),
                new RoadNetworkChangeFeedProjection())
            { }
    }
}
