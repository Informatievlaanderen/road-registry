namespace RoadRegistry.BackOffice.Projections
{
    using System.Text;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Schema;

    public class RoadShapeRunner : Runner<ShapeContext>
    {
        private static readonly Encoding WindowsAnsiEncoding = Encoding.GetEncoding(1252);

        public RoadShapeRunner(EnvelopeFactory envelopeFactory, ILoggerFactory loggerFactory, IBlobClient client, RecyclableMemoryStreamManager manager) :
            base(
                "RoadShapeRunner",
                envelopeFactory,
                loggerFactory.CreateLogger("RoadShapeRunner"),
                new RoadNodeRecordProjection(manager,WindowsAnsiEncoding),
                new RoadSegmentRecordProjection(manager,WindowsAnsiEncoding),
                new RoadSegmentSurfaceAttributeRecordProjection(manager, WindowsAnsiEncoding),
                new RoadSegmentLaneAttributeRecordProjection(manager, WindowsAnsiEncoding),
                new RoadSegmentWidthAttributeRecordProjection(manager, WindowsAnsiEncoding),
                new RoadSegmentEuropeanRoadAttributeRecordProjection(manager, WindowsAnsiEncoding),
                new RoadSegmentNationalRoadAttributeRecordProjection(manager, WindowsAnsiEncoding),
                new RoadSegmentNumberedRoadAttributeRecordProjection(manager, WindowsAnsiEncoding),
                new OrganizationRecordProjection(manager, WindowsAnsiEncoding),
                new GradeSeparatedJunctionRecordProjection(manager, WindowsAnsiEncoding),
                new RoadNetworkInfoProjection(),
                new RoadNetworkChangeFeedProjection(client))
            { }
    }
}
