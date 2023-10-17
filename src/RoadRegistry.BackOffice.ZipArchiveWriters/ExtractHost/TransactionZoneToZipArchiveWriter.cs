namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using System.Text;
using System.Threading;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Extensions;
using Extracts;
using Extracts.Dbase;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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

        var polygon = BackOffice.GeometryTranslator.FromPolygonal(request.Contour);
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

public class TransactionZoneToZipArchiveWriterNetTopologySuite : IZipArchiveWriter<EditorContext>
{
    private readonly Encoding _encoding;
    private const ExtractFileName ExtractFileName = Extracts.ExtractFileName.Transactiezones;

    public TransactionZoneToZipArchiveWriterNetTopologySuite(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request,
        EditorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        await CreateDbaseEntry(archive, ExtractFileName, new[]
        {
            new TransactionZoneDbaseRecord
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
            }
        }, cancellationToken);

        var features = new List<IFeature>
        {
            new Feature((Geometry)request.Contour, new AttributesTable())
        };

        await archive.CreateShapeEntry(ExtractFileName, _encoding, features, WellKnownGeometryFactories.WithoutMAndZ, cancellationToken);
        await archive.CreateCpgEntry(ExtractFileName, _encoding, cancellationToken);
    }

    private async Task CreateDbaseEntry(ZipArchive archive, ExtractFileName fileName, ICollection<TransactionZoneDbaseRecord> dbfRecords, CancellationToken cancellationToken)
    {
        var dbfEntry = archive.CreateEntry(fileName.ToDbaseFileName());
        var dbfHeader = new DbaseFileHeader(
            DateTime.Now,
            DbaseCodePage.Western_European_ANSI,
            new DbaseRecordCount(dbfRecords.Count),
            TransactionZoneDbaseRecord.Schema
        );

        await using var dbfEntryStream = dbfEntry.Open();

        using var dbfWriter = new DbaseBinaryWriter(dbfHeader, new BinaryWriter(dbfEntryStream, _encoding, true));
        dbfWriter.Write(dbfRecords);
        dbfWriter.Writer.Flush();

        await dbfEntryStream.FlushAsync(cancellationToken);
    }
}
