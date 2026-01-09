namespace RoadRegistry.BackOffice.ZipArchiveWriters.ForProduct;

using System.Globalization;
using System.IO.Compression;
using System.Text;
using Abstractions;
using Microsoft.IO;
using NodaTime;
using Product.Schema;
using RoadRegistry.Extracts.Schemas.ExtractV1.Lists;
using RoadRegistry.Infrastructure;

public class RoadNetworkForProductToZipArchiveWriter : IZipArchiveWriter<ProductContext>
{
    private readonly IZipArchiveWriter<ProductContext> _writer;

    public RoadNetworkForProductToZipArchiveWriter(
        LocalDate date,
        ZipArchiveWriterOptions zipArchiveWriterOptions,
        IStreetNameCache streetNameCache,
        RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(zipArchiveWriterOptions);
        ArgumentNullException.ThrowIfNull(streetNameCache);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);

        var versionDirectory = $"Wegenregister_SHAPE_{date.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";
        var extraFileEntryFormat = versionDirectory + "/Shapefile/extra/{0}";
        var shapeFileEntryFormat = versionDirectory + "/Shapefile/{0}";
        var versionFileEntryFormat = versionDirectory + "/{0}";
        var assembly = typeof(RoadNetworkForProductToZipArchiveWriter).Assembly;
        var resourceNameFormat = typeof(RoadNetworkForProductToZipArchiveWriter).Namespace + ".StaticData.{0}";

        _writer = new CompositeZipArchiveWriter<ProductContext>(
            new SnapshotTransactionZipArchiveWriter<ProductContext>(
                new CompositeZipArchiveWriter<ProductContext>(
                    new OrganizationsToZipArchiveWriter(extraFileEntryFormat, encoding),
                    new RoadNodesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new RoadSegmentsToZipArchiveWriter(shapeFileEntryFormat, zipArchiveWriterOptions, streetNameCache, manager, encoding),
                    new RoadSegmentLaneAttributesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new RoadSegmentWidthAttributesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new RoadSegmentSurfaceAttributesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new RoadSegmentNationalRoadAttributesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new RoadSegmentEuropeanRoadAttributesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new RoadSegmentNumberedRoadAttributesToZipArchiveWriter(shapeFileEntryFormat, manager, encoding),
                    new GradeSeparatedJunctionArchiveWriter(shapeFileEntryFormat, manager, encoding)
                )
            ),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegknoopLktType.dbf"), RoadNodeTypeDbaseRecord.Schema, Lists.AllRoadNodeTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegverhardLktType.dbf"), SurfaceTypeDbaseRecord.Schema, Lists.AllSurfaceTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "GenumwegLktRichting.dbf"), NumberedRoadSegmentDirectionDbaseRecord.Schema, Lists.AllNumberedRoadSegmentDirectionDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegsegmentLktWegcat.dbf"), RoadSegmentCategoryDbaseRecord.Schema, Lists.AllRoadSegmentCategoryDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegsegmentLktTgbep.dbf"), RoadSegmentAccessRestrictionDbaseRecord.Schema, Lists.AllRoadSegmentAccessRestrictionDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegsegmentLktMethode.dbf"), RoadSegmentGeometryDrawMethodDbaseRecord.Schema, Lists.AllRoadSegmentGeometryDrawMethodDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegsegmentLktMorf.dbf"), RoadSegmentMorphologyDbaseRecord.Schema, Lists.AllRoadSegmentMorphologyDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "WegsegmentLktStatus.dbf"), RoadSegmentStatusDbaseRecord.Schema, Lists.AllRoadSegmentStatusDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "OgkruisingLktType.dbf"), GradeSeparatedJunctionTypeDbaseRecord.Schema, Lists.AllGradeSeparatedJunctionTypeDbaseRecords, encoding),
            new DbaseFileArchiveWriter<ProductContext>(string.Format(extraFileEntryFormat, "RijstrokenLktRichting.dbf"), LaneDirectionDbaseRecord.Schema, Lists.AllLaneDirectionDbaseRecords, encoding),
            new ProjectionFormatFileZipArchiveWriter<ProductContext>(string.Format(shapeFileEntryFormat, "Wegsegment.prj"), encoding),
            new ProjectionFormatFileZipArchiveWriter<ProductContext>(string.Format(shapeFileEntryFormat, "Wegknoop.prj"), encoding),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Objectcataloog_WR.pdf"), string.Format(versionFileEntryFormat, "Objectcataloog_WR.pdf")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Leesmij_WR.pdf"), string.Format(versionFileEntryFormat, "Leesmij_WR.pdf")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Wegknoop.lyr"), string.Format(shapeFileEntryFormat, "Wegknoop.lyr")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Wegknoop.sld"), string.Format(shapeFileEntryFormat, "Wegknoop.sld")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Wegknoop.WOR"), string.Format(shapeFileEntryFormat, "Wegknoop.WOR")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Wegsegment.lyr"), string.Format(shapeFileEntryFormat, "Wegsegment.lyr")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Wegsegment.sld"), string.Format(shapeFileEntryFormat, "Wegsegment.sld")),
            new EmbeddedResourceZipArchiveWriter<ProductContext>(assembly, string.Format(resourceNameFormat, "Wegsegment.WOR"), string.Format(shapeFileEntryFormat, "Wegsegment.WOR"))
        );
    }

    public Task WriteAsync(ZipArchive archive, ProductContext context, CancellationToken cancellationToken)
    {
        return _writer.WriteAsync(archive, context, cancellationToken);
    }
}
