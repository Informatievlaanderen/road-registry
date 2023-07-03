namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using System.Reflection;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.FeatureCompare;

public class ZipArchiveShapeFileReaderTests
{
    [Theory]
    [InlineData("eWegknoop.zip", ExtractFileName.Wegknoop)]
    public async Task RoadNodeShapeFileShouldHaveSameAmountOfRecordsAsDbaseFile(string resourceName, ExtractFileName entryFileName)
    {
        var sut = new ZipArchiveShapeFileReader();

        using var beforeFcArchiveStream = new MemoryStream();
        await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.Resources.ZipArchiveShapeFileReaderTests.{resourceName}"))
        {
            await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
        }

        beforeFcArchiveStream.Position = 0;

        using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
        {
            var shpEntry = beforeFcArchive.Entries.Single(x => string.Equals(x.Name,  FeatureType.Extract.GetShpFileName(entryFileName), StringComparison.InvariantCultureIgnoreCase));

            var dbfRecords = new SimpleExtractDbfReader<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema).Read(beforeFcArchive.Entries, entryFileName);
            Assert.True(dbfRecords.Any());

            var records = sut.Read(shpEntry).ToArray();
            Assert.Equal(dbfRecords.Count, records.Length);
        }
    }

    [Theory]
    [InlineData("eWegsegment.zip", ExtractFileName.Wegsegment)]
    public async Task RoadSegmentShapeFileShouldHaveSameAmountOfRecordsAsDbaseFile(string resourceName, ExtractFileName entryFileName)
    {
        var sut = new ZipArchiveShapeFileReader();

        using var beforeFcArchiveStream = new MemoryStream();
        await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.Resources.ZipArchiveShapeFileReaderTests.{resourceName}"))
        {
            await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
        }

        beforeFcArchiveStream.Position = 0;

        using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
        {
            var shpEntry = beforeFcArchive.Entries.Single(x => string.Equals(x.Name, FeatureType.Extract.GetShpFileName(entryFileName), StringComparison.InvariantCultureIgnoreCase));

            var dbfRecords = new SimpleExtractDbfReader<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema).Read(beforeFcArchive.Entries, entryFileName);
            Assert.True(dbfRecords.Any());

            var records = sut.Read(shpEntry).ToArray();
            Assert.Equal(dbfRecords.Count, records.Length);
        }
    }

    private class SimpleExtractDbfReader<TDbaseRecord> : DbaseFeatureReader<TDbaseRecord, object>
        where TDbaseRecord : DbaseRecord, new()
    {
        public SimpleExtractDbfReader(DbaseSchema dbaseSchema)
            : base(Encoding.UTF8, dbaseSchema)
        {
        }

        protected override object ConvertDbfRecordToFeature(RecordNumber recordNumber, TDbaseRecord dbaseRecord)
        {
            return dbaseRecord;
        }

        public List<object> Read(IReadOnlyCollection<ZipArchiveEntry> entries, ExtractFileName fileName)
        {
            return Read(entries, FeatureType.Extract, fileName);
        }
    }
}
