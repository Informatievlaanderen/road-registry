namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Extracts;
using Extracts.Dbase.Lists;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

public class RoadNetworkExtractNetTopologySuiteZipArchiveWriter : IZipArchiveWriter
{
    private readonly CompositeZipArchiveWriter _writer;

    public RoadNetworkExtractNetTopologySuiteZipArchiveWriter(
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ILogger<RoadNetworkExtractNetTopologySuiteZipArchiveWriter> logger)
    {
        ArgumentNullException.ThrowIfNull(zipArchiveWriterOptions);
        ArgumentNullException.ThrowIfNull(streetNameCache);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(logger);

        _writer = new CompositeZipArchiveWriter(logger,
            new TransactionZoneNetTopologySuiteZipArchiveWriter(encoding),
            new OrganizationsNetTopologySuiteZipArchiveWriter(encoding),
            new RoadNodesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new RoadSegmentsNetTopologySuiteZipArchiveWriter(streetNameCache, manager, encoding),
            new RoadSegmentLaneAttributesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new RoadSegmentWidthAttributesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new RoadSegmentSurfaceAttributesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new RoadSegmentNationalRoadAttributesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new RoadSegmentEuropeanRoadAttributesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new RoadSegmentNumberedRoadAttributesNetTopologySuiteZipArchiveWriter(manager, encoding),
            new GradeSeparatedJunctionNetTopologySuiteZipArchiveWriter(manager, encoding),
            new IntegrationNetTopologySuiteZipArchiveWriter(streetNameCache, manager, encoding),

            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eGenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eWegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eOgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
            new DbaseFileNetTopologySuiteZipArchiveWriter("eRijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding)
        );
    }

    public Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        CancellationToken cancellationToken)
    {
        return _writer.WriteAsync(archive, request, zipArchiveDataProvider, cancellationToken);
    }
}
