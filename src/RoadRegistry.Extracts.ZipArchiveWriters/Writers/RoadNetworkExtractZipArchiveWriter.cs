namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers;

using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schemas.ExtractV2.Lists;
using RoadRegistry.Infrastructure;

public class RoadNetworkExtractZipArchiveWriter : IZipArchiveWriter
{
    private readonly CompositeZipArchiveWriter _writer;

    public RoadNetworkExtractZipArchiveWriter(
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(zipArchiveWriterOptions);
        ArgumentNullException.ThrowIfNull(streetNameCache);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _writer = new CompositeZipArchiveWriter(loggerFactory.CreateLogger(GetType()),
            new TransactionZoneZipArchiveWriter(encoding),
            new OrganizationsZipArchiveWriter(encoding),
            new RoadNodesZipArchiveWriter(encoding),
            new RoadSegmentsZipArchiveWriter(streetNameCache, encoding),
            new RoadSegmentSurfaceAttributesZipArchiveWriter(encoding),
            new RoadSegmentNationalRoadAttributesZipArchiveWriter(encoding),
            new RoadSegmentEuropeanRoadAttributesZipArchiveWriter(encoding),
            new GradeSeparatedJunctionZipArchiveWriter(encoding),
            new IntegrationZipArchiveWriter(streetNameCache, encoding),

            new DbaseFileZipArchiveWriter("eWegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eOgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding)
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
