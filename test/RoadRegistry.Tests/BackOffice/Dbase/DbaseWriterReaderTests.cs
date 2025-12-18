namespace RoadRegistry.Tests.BackOffice.Dbase;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.Extracts.Schemas.ExtractV1;

public class DbaseWriterReaderTests
{
    [Fact]
    public async Task WhenWrite_ThenReadSucceeds()
    {
        var encoding = Encoding.UTF8;
        var extractFileName = ExtractFileName.Transactiezones;
        var featureType = FeatureType.Change;
        var dbaseSchema = TransactionZoneDbaseRecord.Schema;

        var dbfRecords = new DbaseRecord[] {
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

        var writer = new DbaseRecordWriter(encoding);
        await writer.WriteToArchive(
            archive,
            extractFileName,
            featureType,
            dbaseSchema,
            dbfRecords,
            CancellationToken.None);

        var reader = new DbaseRecordReader(encoding);
        var dbase = reader.ReadFromArchive<TransactionZoneDbaseRecord>(archive, extractFileName, featureType, dbaseSchema);

        var readDbaseRecords = new List<TransactionZoneDbaseRecord>();
        while (dbase.RecordEnumerator!.MoveNext())
        {
            readDbaseRecords.Add(dbase.RecordEnumerator.Current);
        }

        readDbaseRecords[0].SOURCEID.Value.Should().Be(1);
        readDbaseRecords[1].SOURCEID.Value.Should().Be(2);
    }
}
