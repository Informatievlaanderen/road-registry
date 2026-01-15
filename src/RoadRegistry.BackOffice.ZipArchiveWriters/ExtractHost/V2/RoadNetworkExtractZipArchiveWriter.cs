namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V2;

using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schemas.ExtractV1.Lists;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Infrastructure;
using CompositeZipArchiveWriter = ExtractHost.CompositeZipArchiveWriter;
using IZipArchiveDataProvider = ExtractHost.IZipArchiveDataProvider;
using IZipArchiveWriter = ExtractHost.IZipArchiveWriter;

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
            new RoadNodesZipArchiveWriter(manager, encoding),
            new RoadSegmentsZipArchiveWriter(streetNameCache, manager, encoding),
            new RoadSegmentLaneAttributesZipArchiveWriter(manager, encoding),
            new RoadSegmentWidthAttributesZipArchiveWriter(manager, encoding),
            new RoadSegmentSurfaceAttributesZipArchiveWriter(manager, encoding),
            new RoadSegmentNationalRoadAttributesZipArchiveWriter(manager, encoding),
            new RoadSegmentEuropeanRoadAttributesZipArchiveWriter(manager, encoding),
            new RoadSegmentNumberedRoadAttributesZipArchiveWriter(manager, encoding),
            new GradeSeparatedJunctionZipArchiveWriter(manager, encoding),
            new IntegrationZipArchiveWriter(streetNameCache, manager, encoding),

            new DbaseFileZipArchiveWriter("eWegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eGenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eWegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eOgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
            new DbaseFileZipArchiveWriter("eRijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding)
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
