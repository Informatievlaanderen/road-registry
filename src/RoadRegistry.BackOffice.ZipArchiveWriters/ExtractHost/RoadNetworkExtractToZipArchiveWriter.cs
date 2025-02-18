namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Extracts;
using Extracts.Dbase.Lists;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

public class RoadNetworkExtractToZipArchiveWriter : IZipArchiveWriter
{
    private readonly CompositeZipArchiveWriter _writer;

    public RoadNetworkExtractToZipArchiveWriter(
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ILogger<RoadNetworkExtractToZipArchiveWriter> logger)
    {
        ArgumentNullException.ThrowIfNull(zipArchiveWriterOptions);
        ArgumentNullException.ThrowIfNull(streetNameCache);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(logger);

        _writer = new CompositeZipArchiveWriter(logger,
            new CompositeZipArchiveWriter(logger,
                new TransactionZoneToZipArchiveWriter(encoding),
                new OrganizationsToZipArchiveWriter(encoding),
                new RoadNodesToZipArchiveWriter(manager, encoding),
                new RoadSegmentsToZipArchiveWriter(zipArchiveWriterOptions, streetNameCache, manager, encoding),
                new RoadSegmentLaneAttributesToZipArchiveWriter(manager, encoding),
                new RoadSegmentWidthAttributesToZipArchiveWriter(manager, encoding),
                new RoadSegmentSurfaceAttributesToZipArchiveWriter(manager, encoding),
                new RoadSegmentNationalRoadAttributesToZipArchiveWriter(manager, encoding),
                new RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(manager, encoding),
                new RoadSegmentNumberedRoadAttributesToZipArchiveWriter(manager, encoding),
                new GradeSeparatedJunctionArchiveWriter(manager, encoding),
                new IntegrationToZipArchiveWriter(zipArchiveWriterOptions, streetNameCache, manager, encoding)
            ),
            new DbaseFileArchiveWriter("eWegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eWegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eGenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eWegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eWegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eWegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eWegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eWegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eOgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter("eRijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding)
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
