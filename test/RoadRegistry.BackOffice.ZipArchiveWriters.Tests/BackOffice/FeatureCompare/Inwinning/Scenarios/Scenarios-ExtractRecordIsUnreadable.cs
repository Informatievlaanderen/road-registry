namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.Inwinning.Scenarios;

using System.IO.Compression;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.FeatureCompare.Inwinning;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.Inwinning;
using Xunit.Abstractions;

// What an extract record the reader stumbles over does to the rest of the file. A record it cannot read stops it
// where it stands, and every record behind it is never seen - which the extract's own record problems, dropped
// because the system generates those files, then say nothing about. The feature compare goes on to work from an
// extract it believes is complete: a road segment it never read is never matched and never removed, and an upload
// that redraws such a road adds its road next to the one that stayed behind.
public class ExtractRecordIsUnreadableScenarios : FeatureCompareTranslatorScenariosBase
{
    private const string ExtractRoadSegmentsDbaseFileName = "eWegsegment.dbf";

    public ExtractRecordIsUnreadableScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    // CREATIE and VERSIE are read by nobody - an upload is judged on its identifiers, its geometry and its
    // attributes - so a date in a format they are not written in is no reason to stop reading the file. The
    // records that carry one came out of the published product, where the dates are plain ISO dates.
    [Fact]
    public async Task WhenAnExtractRecordHasADateThatCannotBeRead_ThenTheUploadMeansTheSame()
    {
        var upload = BuildArchiveBytes();

        var expected = await Translate(Open(upload));
        var actual = await Translate(Open(upload, dbase => OverwriteFieldOfLastRecord(dbase, "CREATIE", "2026-11-09")));

        actual.Select(Describe).Should().BeEquivalentTo(expected.Select(Describe));
    }

    // Read in full, an extract can turn out to claim one temp id twice. The road numbers and the crossings point
    // at their road segment by temp id, so the mapping they go through has no answer for that and threw. Said up
    // front instead, where the extract is read.
    [Fact]
    public async Task WhenTwoExtractRecordsShareATempId_ThenItIsReported()
    {
        var archive = Open(BuildArchiveBytes(), dbase => CopyFieldOfFirstRecordToLastRecord(dbase, "WS_TEMPID"));
        var sut = ZipArchiveFeatureCompareTranslatorV3Builder.Create();

        var act = () => sut.TranslateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ZipArchiveValidationException>();
        exception.Which.Problems.Select(x => x.Reason).Should()
            .Contain(nameof(DbaseFileProblems.IdentifierNotUnique));
    }

    private static string Describe(RoadRegistry.ScopedRoadNetwork.IRoadNetworkChange change)
    {
        return change.GetType().Name + " " + change;
    }

    private async Task<TranslatedChanges> Translate(ZipArchive archive)
    {
        return await TranslateSucceeds(archive);
    }

    // One upload, so that the patched copy differs from the untouched one in nothing but the patch: the builder
    // fills everything else from a random fixture.
    private static byte[] BuildArchiveBytes()
    {
        // The road numbers and the crossings are left out of the extract: they are what points at a road segment
        // by temp id, and a pointer that no longer lands is reported in its own right, which is not what is tested.
        return new InwinningZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                builder.DataSet.EuropeanRoadDbaseRecords.Clear();
                builder.DataSet.NationalRoadDbaseRecords.Clear();
                builder.DataSet.GradeSeparatedJunctionDbaseRecords.Clear();
            })
            .BuildArchiveStream()
            .ToArray();
    }

    private static ZipArchive Open(byte[] upload, Action<byte[]>? patchExtractRoadSegments = null)
    {
        var archiveStream = new MemoryStream();
        archiveStream.Write(upload, 0, upload.Length);
        archiveStream.Position = 0;

        if (patchExtractRoadSegments is not null)
        {
            PatchExtractRoadSegments(archiveStream, patchExtractRoadSegments);
        }

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, true);
    }

    private static void PatchExtractRoadSegments(MemoryStream archiveStream, Action<byte[]> patch)
    {
        using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true))
        {
            var entry = archive.Entries.SingleOrDefault(x =>
                string.Equals(x.FullName, ExtractRoadSegmentsDbaseFileName, StringComparison.OrdinalIgnoreCase));
            entry.Should().NotBeNull($"the archive is expected to hold a '{ExtractRoadSegmentsDbaseFileName}'");

            byte[] dbase;
            using (var entryStream = entry!.Open())
            using (var buffer = new MemoryStream())
            {
                entryStream.CopyTo(buffer);
                dbase = buffer.ToArray();
            }

            patch(dbase);

            using (var entryStream = entry.Open())
            {
                entryStream.SetLength(0);
                entryStream.Write(dbase, 0, dbase.Length);
            }
        }

        archiveStream.Position = 0;
    }

    private static void OverwriteFieldOfLastRecord(byte[] dbase, string fieldName, string value)
    {
        var (offset, length) = LocateFieldOfLastRecord(dbase, fieldName);

        Encoding.ASCII.GetBytes(value.PadRight(length)).CopyTo(dbase, offset);
    }

    private static void CopyFieldOfFirstRecordToLastRecord(byte[] dbase, string fieldName)
    {
        var (lastOffset, length) = LocateFieldOfLastRecord(dbase, fieldName);
        var recordLength = BitConverter.ToUInt16(dbase, 10);
        var recordCount = BitConverter.ToInt32(dbase, 4);

        Array.Copy(dbase, lastOffset - (recordCount - 1) * recordLength, dbase, lastOffset, length);
    }

    // The field descriptors follow the 32-byte header, 32 bytes each, up to the 0x0D that closes them off. Each one
    // names its field in its first 11 bytes and gives its length at offset 16; the fields sit in the record in that
    // same order, behind the one byte that marks the record as deleted or not.
    private static (int Offset, int Length) LocateFieldOfLastRecord(byte[] dbase, string fieldName)
    {
        var recordCount = BitConverter.ToInt32(dbase, 4);
        var headerLength = BitConverter.ToUInt16(dbase, 8);
        var recordLength = BitConverter.ToUInt16(dbase, 10);

        recordCount.Should().BeGreaterThan(1, "the extract is expected to hold more than one road segment");

        var fieldOffsetInRecord = 1;
        var descriptorOffset = 32;
        while (dbase[descriptorOffset] != 0x0D)
        {
            var name = Encoding.ASCII.GetString(dbase, descriptorOffset, 11).TrimEnd('\0');
            var fieldLength = dbase[descriptorOffset + 16];

            if (name == fieldName)
            {
                return (headerLength + (recordCount - 1) * recordLength + fieldOffsetInRecord, fieldLength);
            }

            fieldOffsetInRecord += fieldLength;
            descriptorOffset += 32;
        }

        throw new InvalidOperationException($"Field '{fieldName}' was not found in the dbase file.");
    }
}
