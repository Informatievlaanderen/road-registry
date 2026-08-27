namespace RoadRegistry.Tests.BackOffice.ShapeFile;

using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Schemas.ExtractV1;
using EsriShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class ShapeFileWriterReaderTests
{
    private const int ShapeFileHeaderLengthInBytes = 100;
    private const int ShapeIndexRecordLengthInBytes = 8;

    [Fact]
    public async Task WhenWrite_ThenReadSucceeds()
    {
        var encoding = Encoding.UTF8;
        var extractFileName = ExtractFileName.Transactiezones;
        var featureType = FeatureType.Change;
        var dbaseSchema = TransactionZoneDbaseRecord.Schema;

        var dbfRecords = new[]
        {
            new TransactionZoneDbaseRecord
            {
                SOURCEID = { Value = 1 },
                APPLICATIE = { Value = "Wegenregister" }
            },
            new TransactionZoneDbaseRecord
            {
                SOURCEID = { Value = 2 },
                APPLICATIE = { Value = "Wegenregister" }
            }
        };

        using var archiveStream = new MemoryStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);

        var writer = new ShapeFileRecordWriter(encoding);
        await writer.WriteToArchive(
            archive,
            extractFileName,
            featureType,
            EsriShapeType.Polygon,
            dbaseSchema,
            dbfRecords.Select(dbfRecord => ((DbaseRecord)dbfRecord, new WKTReader().Read("MULTIPOLYGON(((55000 200000,55000 200100,55100 200100,55100 200000,55000 200000)))"))),
            CancellationToken.None);

        var reader = new ShapeFileRecordReader(encoding);
        var dbase = reader.ReadFromArchive<TransactionZoneDbaseRecord>(archive, extractFileName, featureType, dbaseSchema, WellKnownGeometryFactories.Lambert72);

        var readDbaseRecords = new List<(TransactionZoneDbaseRecord, Geometry)>();
        while (dbase.RecordEnumerator!.MoveNext())
        {
            readDbaseRecords.Add(dbase.RecordEnumerator.Current);
        }

        readDbaseRecords[0].Item1.SOURCEID.Value.Should().Be(1);
        readDbaseRecords[0].Item2.Should().NotBeNull();
        readDbaseRecords[0].Item2.Should().BeOfType<MultiPolygon>();
        readDbaseRecords[1].Item1.SOURCEID.Value.Should().Be(2);
        readDbaseRecords[1].Item2.Should().NotBeNull();
        readDbaseRecords[1].Item2.Should().BeOfType<MultiPolygon>();
    }

    [Theory]
    [InlineData(EsriShapeType.Point)]
    [InlineData(EsriShapeType.PolyLine)]
    [InlineData(EsriShapeType.Polygon)]
    public async Task WhenWriteWithoutRecords_ThenShapeFileHeadersDescribeAnEmptyShapeFile(EsriShapeType shapeType)
    {
        var encoding = Encoding.UTF8;
        var extractFileName = ExtractFileName.Transactiezones;
        var featureType = FeatureType.Change;

        using var archiveStream = new MemoryStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);

        var writer = new ShapeFileRecordWriter(encoding);
        await writer.WriteToArchive(
            archive,
            extractFileName,
            featureType,
            shapeType,
            TransactionZoneDbaseRecord.Schema,
            [],
            CancellationToken.None);

        var shpHeader = ReadShapeFileHeader(archive, extractFileName.ToShapeFileName(featureType));
        var shxHeader = ReadShapeFileHeader(archive, extractFileName.ToShapeIndexFileName(featureType));

        // the file length only covers the 100 byte header itself, expressed in 16-bit words
        shpHeader.FileLength.Should().Be(ShapeFileHeader.Length);
        shxHeader.FileLength.Should().Be(ShapeFileHeader.Length);

        // this is how GDAL (QGIS, ArcGIS, ...) determines the number of shapes
        var numberOfShapes = (shxHeader.FileLength.ToInt32() * 2 - ShapeFileHeaderLengthInBytes) / ShapeIndexRecordLengthInBytes;
        numberOfShapes.Should().Be(0);
        numberOfShapes.Should().Be(ReadDbaseRecordCount(archive, extractFileName.ToDbaseFileName(featureType)));
    }

    [Fact]
    public async Task WhenWriteWithoutRecords_ThenReadSucceeds()
    {
        var encoding = Encoding.UTF8;
        var extractFileName = ExtractFileName.Transactiezones;
        var featureType = FeatureType.Change;
        var dbaseSchema = TransactionZoneDbaseRecord.Schema;

        using var archiveStream = new MemoryStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);

        var writer = new ShapeFileRecordWriter(encoding);
        await writer.WriteToArchive(
            archive,
            extractFileName,
            featureType,
            EsriShapeType.Polygon,
            dbaseSchema,
            [],
            CancellationToken.None);

        var reader = new ShapeFileRecordReader(encoding);
        var dbase = reader.ReadFromArchive<TransactionZoneDbaseRecord>(archive, extractFileName, featureType, dbaseSchema, WellKnownGeometryFactories.Lambert72);

        dbase.RecordEnumerator!.MoveNext().Should().BeFalse();
    }

    [Fact]
    public async Task WhenWriteWithRecords_ThenShapeFileHeadersMatchTheFileLength()
    {
        var encoding = Encoding.UTF8;
        var extractFileName = ExtractFileName.Transactiezones;
        var featureType = FeatureType.Change;

        using var archiveStream = new MemoryStream();
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);

        var writer = new ShapeFileRecordWriter(encoding);
        await writer.WriteToArchive(
            archive,
            extractFileName,
            featureType,
            EsriShapeType.Polygon,
            TransactionZoneDbaseRecord.Schema,
            [
                ((DbaseRecord)new TransactionZoneDbaseRecord
                {
                    SOURCEID = { Value = 1 },
                    APPLICATIE = { Value = "Wegenregister" }
                }, new WKTReader().Read("MULTIPOLYGON(((55000 200000,55000 200100,55100 200100,55100 200000,55000 200000)))"))
            ],
            CancellationToken.None);

        var shpFileName = extractFileName.ToShapeFileName(featureType);
        var shxFileName = extractFileName.ToShapeIndexFileName(featureType);

        ReadShapeFileHeader(archive, shpFileName).FileLength.ToInt32().Should().Be((int)ReadEntry(archive, shpFileName).Length / 2);
        ReadShapeFileHeader(archive, shxFileName).FileLength.ToInt32().Should().Be((int)ReadEntry(archive, shxFileName).Length / 2);

        var numberOfShapes = (ReadShapeFileHeader(archive, shxFileName).FileLength.ToInt32() * 2 - ShapeFileHeaderLengthInBytes) / ShapeIndexRecordLengthInBytes;
        numberOfShapes.Should().Be(1);
    }

    private static ShapeFileHeader ReadShapeFileHeader(ZipArchive archive, string fileName)
    {
        using var stream = ReadEntry(archive, fileName);
        using var reader = new BinaryReader(stream);
        return ShapeFileHeader.Read(reader);
    }

    private static int ReadDbaseRecordCount(ZipArchive archive, string fileName)
    {
        using var stream = ReadEntry(archive, fileName);
        var header = new byte[8];
        stream.ReadExactly(header);
        return BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4));
    }

    private static MemoryStream ReadEntry(ZipArchive archive, string fileName)
    {
        var entry = archive.GetEntry(fileName);
        entry.Should().NotBeNull($"{fileName} should be present in the archive");

        var stream = new MemoryStream();
        using (var entryStream = entry!.Open())
        {
            entryStream.CopyTo(stream);
        }

        stream.Position = 0;
        return stream;
    }
}
