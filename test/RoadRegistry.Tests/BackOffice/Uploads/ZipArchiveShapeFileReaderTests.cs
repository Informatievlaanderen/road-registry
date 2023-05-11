namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using System.Reflection;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.FeatureCompare;

public class ZipArchiveShapeFileReaderTests
{
    [Theory]
    [InlineData("eWegknoop.zip", "eWegknoop")]
    public async Task RoadNodeShapeFileShouldHaveSameAmountOfRecordsAsDbaseFile(string resourceName, string entryFileName)
    {
        var sut = new ZipArchiveShapeFileReader();

        using (var beforeFcArchiveStream = new MemoryStream())
        {
            await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.Resources.ZipArchiveShapeFileReaderTests.{resourceName}"))
            {
                await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
            }

            beforeFcArchiveStream.Position = 0;

            using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
            {
                var shpEntry = beforeFcArchive.Entries.Single(x => string.Equals(x.Name, $"{entryFileName}.shp", StringComparison.InvariantCultureIgnoreCase));

                var dbfRecords = new SimpleDbfReader<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema)
                    .Read(beforeFcArchive.Entries, entryFileName);
                Assert.True(dbfRecords.Any());

                var records = sut.Read(shpEntry).ToArray();
                Assert.Equal(dbfRecords.Count, records.Length);
            }
        }
    }

    [Theory]
    [InlineData("eWegsegment.zip", "eWegsegment")]
    public async Task RoadSegmentShapeFileShouldHaveSameAmountOfRecordsAsDbaseFile(string resourceName, string entryFileName)
    {
        var sut = new ZipArchiveShapeFileReader();

        using (var beforeFcArchiveStream = new MemoryStream())
        {
            await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.Resources.ZipArchiveShapeFileReaderTests.{resourceName}"))
            {
                await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
            }

            beforeFcArchiveStream.Position = 0;

            using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
            {
                var shpEntry = beforeFcArchive.Entries.Single(x => string.Equals(x.Name, $"{entryFileName}.shp", StringComparison.InvariantCultureIgnoreCase));

                var dbfRecords = new SimpleDbfReader<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema)
                    .Read(beforeFcArchive.Entries, entryFileName);
                Assert.True(dbfRecords.Any());

                var records = sut.Read(shpEntry).ToArray();
                Assert.Equal(dbfRecords.Count, records.Length);
            }
        }
    }

    private class SimpleDbfReader<TDbaseRecord> : DbaseFeatureReader<TDbaseRecord, object>
        where TDbaseRecord : DbaseRecord, new()
    {
        public SimpleDbfReader(DbaseSchema dbaseSchema)
            : base(Encoding.UTF8, dbaseSchema)
        {
        }

        public List<object> Read(IReadOnlyCollection<ZipArchiveEntry> entries, string fileName)
        {
            return Read(entries, FeatureType.Levering, fileName);
        }

        protected override object ConvertDbfRecordToFeature(RecordNumber recordNumber, TDbaseRecord dbaseRecord)
        {
            return dbaseRecord;
        }
    }
}
