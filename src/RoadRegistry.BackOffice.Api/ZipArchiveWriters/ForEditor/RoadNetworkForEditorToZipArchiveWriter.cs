namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForEditor
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.IO;
    using Editor.Schema;
    using Editor.Schema.Lists;

    public class RoadNetworkForEditorToZipArchiveWriter : IZipArchiveWriter<EditorContext>
    {
        private readonly IZipArchiveWriter<EditorContext> _writer;

        public RoadNetworkForEditorToZipArchiveWriter(
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
                        new OrganizationsToZipArchiveWriter(manager, encoding),
                        new RoadNodesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentsToZipArchiveWriter(zipArchiveWriterOptions, streetNameCache, manager, encoding),
                        new RoadSegmentLaneAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentWidthAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentSurfaceAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentNationalRoadAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentNumberedRoadAttributesToZipArchiveWriter(manager, encoding),
                        new GradeSeparatedJunctionArchiveWriter(manager, encoding)
                    )
                ),
                new DbaseFileArchiveWriter<EditorContext>("WegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("WegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("GenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("WegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("WegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("WegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("WegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("WegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("OgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter<EditorContext>("RijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding)
            );
        }

        public Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(archive, context, cancellationToken);
        }
    }
}
