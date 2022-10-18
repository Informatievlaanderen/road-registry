namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Dbase.Lists;
using Editor.Schema;
using Extracts;
using Microsoft.IO;

public class RoadNetworkExtractToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly IZipArchiveWriter<EditorContext> _writer;

    public RoadNetworkExtractToZipArchiveWriter(
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        if (zipArchiveWriterOptions == null) throw new ArgumentNullException(nameof(zipArchiveWriterOptions));

        if (streetNameCache == null) throw new ArgumentNullException(nameof(streetNameCache));

        if (manager == null) throw new ArgumentNullException(nameof(manager));

        if (encoding == null) throw new ArgumentNullException(nameof(encoding));

        _writer = new CompositeZipArchiveWriter<EditorContext>(
            new ReadCommittedZipArchiveWriter<EditorContext>(
                new CompositeZipArchiveWriter<EditorContext>(
                    new TransactionZoneToZipArchiveWriter(encoding),
                    new OrganizationsToZipArchiveWriter(manager, encoding),
                    new RoadNodesToZipArchiveWriter(manager, encoding),
                    new RoadSegmentsToZipArchiveWriter(zipArchiveWriterOptions, streetNameCache, manager, encoding),
                    new RoadSegmentLaneAttributesToZipArchiveWriter(manager, encoding),
                    new RoadSegmentWidthAttributesToZipArchiveWriter(manager, encoding),
                    new RoadSegmentSurfaceAttributesToZipArchiveWriter(manager, encoding),
                    new RoadSegmentNationalRoadAttributesToZipArchiveWriter(manager, encoding),
                    new RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(manager, encoding),
                    new RoadSegmentNumberedRoadAttributesToZipArchiveWriter(manager, encoding),
                    new GradeSeparatedJunctionArchiveWriter(manager, encoding),
                    new IntegrationRoadNodesToZipArchiveWriter(manager, encoding),
                    new IntegrationRoadSegmentsToZipArchiveWriter(zipArchiveWriterOptions, streetNameCache, manager, encoding)
                )
            ),
            new DbaseFileArchiveWriter<EditorContext>("eWegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eWegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eGenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eWegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eWegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eWegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eWegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eWegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eOgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter<EditorContext>("eRijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding),
            new ProjectionFormatFileZipArchiveWriter<EditorContext>("eWegsegment.prj", encoding),
            new ProjectionFormatFileZipArchiveWriter<EditorContext>("eWegknoop.prj", encoding),
            new ProjectionFormatFileZipArchiveWriter<EditorContext>("Transactiezones.prj", encoding),
            new ProjectionFormatFileZipArchiveWriter<EditorContext>("iWegknoop.prj", encoding),
            new ProjectionFormatFileZipArchiveWriter<EditorContext>("iWegsegment.prj", encoding)
        );
    }

    public Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, EditorContext context,
        CancellationToken cancellationToken)
    {
        return _writer.WriteAsync(archive, request, context, cancellationToken);
    }
}