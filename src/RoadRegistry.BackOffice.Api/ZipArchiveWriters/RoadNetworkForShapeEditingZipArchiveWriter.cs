namespace RoadRegistry.Api.ZipArchiveWriters
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Schema;
    using BackOffice.Schema.ReferenceData;

    public class RoadNetworkForShapeEditingZipArchiveWriter : IZipArchiveWriter
    {
        private readonly IZipArchiveWriter _writer;

        public RoadNetworkForShapeEditingZipArchiveWriter(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            _writer = new CompositeZipArchiveWriter(
                new ReadCommittedZipArchiveWriter(
                    new CompositeZipArchiveWriter(
                        new OrganizationsToZipArchiveWriter(encoding),
                        new RoadNodesToZipArchiveWriter(encoding),
                        new RoadSegmentsToZipArchiveWriter(encoding),
                        new RoadSegmentLaneAttributesToZipArchiveWriter(encoding),
                        new RoadSegmentWidthAttributesToZipArchiveWriter(encoding),
                        new RoadSegmentSurfaceAttributesToZipArchiveWriter(encoding),
                        new RoadSegmentNationalRoadAttributesToZipArchiveWriter(encoding),
                        new RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(encoding),
                        new RoadSegmentNumberedRoadAttributesToZipArchiveWriter(encoding),
                        new GradeSeperatedJunctionArchiveWriter(encoding)
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

        public Task WriteAsync(ZipArchive archive, ShapeContext context, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(archive, context, cancellationToken);
        }
    }
}
