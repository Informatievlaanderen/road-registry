namespace RoadRegistry.Projections
{
    using System.Text;
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;
    using Aiv.Vbr.Shaperon;
    using BackOffice.Schema;
    using Messages;

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
                new RoadNetworkInfoProjection(reader))
            { }
    }
}
