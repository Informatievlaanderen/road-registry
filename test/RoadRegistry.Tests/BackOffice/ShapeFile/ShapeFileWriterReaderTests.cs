namespace RoadRegistry.Tests.BackOffice.ShapeFile;

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

public class ShapeFileWriterReaderTests
{
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
            NetTopologySuite.IO.Esri.ShapeType.Polygon,
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
}
