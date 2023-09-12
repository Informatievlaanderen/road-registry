namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using System.Threading;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Editor.Schema;
using Extensions;
using Extracts;
using Extracts.Dbase;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Streams;
using DbaseFileHeader = Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader;

public class TransactionZoneToZipArchiveWriter : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;

    public TransactionZoneToZipArchiveWriter(Encoding encoding)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
        EditorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var dbfEntry = archive.CreateEntry("Transactiezones.dbf");
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(1),
            TransactionZoneDbaseRecord.Schema
        );
        await using (var dbfEntryStream = dbfEntry.Open())
        using (var dbfWriter =
               new DbaseBinaryWriter(
                   dbfHeader,
                   new BinaryWriter(dbfEntryStream, _encoding, true)))
        {
            var dbfRecord = new TransactionZoneDbaseRecord
            {
                SOURCEID = { Value = 1 },
                TYPE = { Value = 2 },
                BESCHRIJV =
                {
                    Value = string.IsNullOrEmpty(request.ExtractDescription) ? request.ExternalRequestId : request.ExtractDescription
                },
                OPERATOR = { Value = "" },
                ORG = { Value = "AGIV" },
                APPLICATIE = { Value = "Wegenregister" },
                DOWNLOADID = { Value = request.DownloadId.ToGuid().ToString("N") }
            };
            dbfWriter.Write(dbfRecord);
            dbfWriter.Writer.Flush();
            await dbfEntryStream.FlushAsync(cancellationToken);
        }

        var polygon = PolygonalGeometryTranslator.FromGeometry(request.Contour);
        var shapeContent = new PolygonShapeContent(polygon);
        var shpBoundingBox = BoundingBox3D.FromGeometry(polygon);

        var shpEntry = archive.CreateEntry("Transactiezones.shp");
        var shpHeader = new ShapeFileHeader(
            shapeContent.ContentLength,
            ShapeType.Polygon,
            shpBoundingBox);
        await using (var shpEntryStream = shpEntry.Open())
        using (var shpWriter =
               new ShapeBinaryWriter(
                   shpHeader,
                   new BinaryWriter(shpEntryStream, _encoding, true)))
        {
            shpWriter.Write(shapeContent.RecordAs(RecordNumber.Initial));
            shpWriter.Writer.Flush();
            await shpEntryStream.FlushAsync(cancellationToken);
        }

        var shxEntry = archive.CreateEntry("Transactiezones.shx");
        var shxHeader = shpHeader.ForIndex(new ShapeRecordCount(1));
        await using (var shxEntryStream = shxEntry.Open())
        using (var shxWriter =
               new ShapeIndexBinaryWriter(
                   shxHeader,
                   new BinaryWriter(shxEntryStream, _encoding, true)))
        {
            shxWriter.Write(shapeContent.RecordAs(RecordNumber.Initial).IndexAt(ShapeIndexRecord.InitialOffset));
            shxWriter.Writer.Flush();
            await shxEntryStream.FlushAsync(cancellationToken);
        }

        await archive.CreateCpgEntry("Transactiezones.cpg", _encoding, cancellationToken);
    }
}


public class ZipArchiveShapeFileWriter
{
    private readonly Encoding _encoding;

    public ZipArchiveShapeFileWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public async Task Write(ZipArchiveEntry entry, IList<IFeature> features)
    {
        //OLD
        //var shpHeader = new ShapeFileHeader(
        //    shapeContent.ContentLength,
        //    ShapeType.Polygon,
        //    shpBoundingBox);
        //await using (var shpEntryStream = entry.Open())
        //using (var shpWriter =
        //       new ShapeBinaryWriter(
        //           shpHeader,
        //           new BinaryWriter(shpEntryStream, _encoding, true)))
        //{
        //    shpWriter.Write(shapeContent.RecordAs(RecordNumber.Initial));
        //    shpWriter.Writer.Flush();
        //    await shpEntryStream.FlushAsync(cancellationToken);
        //}

        await using (var shpEntryStream = entry.Open())
        {

            //var attributesTable = new AttributesTable { { "Foo", "Bar" } };
            //var features = new List<IFeature>
            //{
            //    new Feature(contour, attributesTable)
            //};
            
            var streamProvider = new ExternallyManagedStreamProvider(StreamTypes.Shape, shpEntryStream);
            var streamProviderRegistry = new ShapefileStreamProviderRegistry(streamProvider, null);

            var writer = new ShapefileDataWriter(streamProviderRegistry, GeometryConfiguration.GeometryFactory, _encoding);
            var outDbaseHeader = ShapefileDataWriter.GetHeader(features[0], features.Count);
            writer.Header = outDbaseHeader;
            writer.Write(features);
        }
    }
}
