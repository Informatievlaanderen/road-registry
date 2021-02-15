namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForProduct
{
    using System;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using Microsoft.IO;
    using Product.Schema;
    using Product.Schema.Lists;

    public class RoadNetworkForProductToZipArchiveWriter : IZipArchiveWriter<ProductContext>
    {
        private readonly IZipArchiveWriter<ProductContext> _writer;

        public RoadNetworkForProductToZipArchiveWriter(
            ZipArchiveWriterOptions zipArchiveWriterOptions,
            IStreetNameCache streetNameCache,
            RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (zipArchiveWriterOptions == null) throw new ArgumentNullException(nameof(zipArchiveWriterOptions));
            if (streetNameCache == null) throw new ArgumentNullException(nameof(streetNameCache));
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            _writer = new CompositeZipArchiveWriter<ProductContext>(
                new ReadCommittedZipArchiveWriter<ProductContext>(
                    new CompositeZipArchiveWriter<ProductContext>(
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
                new DbaseFileArchiveWriter<ProductContext>("WegknoopLktType.dbf", RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("WegverhardLktType.dbf", SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("GenumwegLktRichting.dbf", NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("WegsegmentLktWegcat.dbf", RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("WegsegmentLktTgbep.dbf", RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("WegsegmentLktMethode.dbf", RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("WegsegmentLktMorf.dbf", RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("WegsegmentLktStatus.dbf", RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("OgkruisingLktType.dbf", GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
                new DbaseFileArchiveWriter<ProductContext>("RijstrokenLktRichting.dbf", LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding),
                new ProjectionFormatFileZipArchiveWriter<ProductContext>("Wegsegment.prj", encoding),
                new ProjectionFormatFileZipArchiveWriter<ProductContext>("Wegknoop.prj", encoding)
            );
        }

        public Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(archive, context, cancellationToken);
        }
    }
}
