namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Editor.Schema;
    using Editor.Schema.Lists;
    using Microsoft.IO;

    public class RoadNetworkForShapeEditingZipArchiveWriter : IZipArchiveWriter
    {
        private readonly IZipArchiveWriter _writer;

        public RoadNetworkForShapeEditingZipArchiveWriter(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            _writer = new CompositeZipArchiveWriter(
                new ReadCommittedZipArchiveWriter(
                    new CompositeZipArchiveWriter(
                        new OrganizationsToZipArchiveWriter(manager, encoding),
                        new RoadNodesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentsToZipArchiveWriter(manager, encoding),
                        new RoadSegmentLaneAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentWidthAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentSurfaceAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentNationalRoadAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(manager, encoding),
                        new RoadSegmentNumberedRoadAttributesToZipArchiveWriter(manager, encoding),
                        new GradeSeparatedJunctionArchiveWriter(manager, encoding)
                    )
                ),
                new DbaseFileArchiveWriter("WegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter("WegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter("GenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
                new DbaseFileArchiveWriter("WegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
                new DbaseFileArchiveWriter("WegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
                new DbaseFileArchiveWriter("WegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
                new DbaseFileArchiveWriter("WegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
                new DbaseFileArchiveWriter("WegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
                new DbaseFileArchiveWriter("OgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter("RijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding)
            );
        }

        public Task WriteAsync(ZipArchive archive, EditorContext context, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(archive, context, cancellationToken);
        }
    }
}
