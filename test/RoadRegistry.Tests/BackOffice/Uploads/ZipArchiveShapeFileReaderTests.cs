namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using System.Reflection;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.FeatureCompare.V1;
using RoadRegistry.BackOffice.ShapeFile;
using RoadRegistry.BackOffice.Uploads;

public class ZipArchiveShapeFileReaderTests
{
    [Theory]
    [InlineData("eWegknoop.zip", ExtractFileName.Wegknoop)]
    public async Task RoadNodeShapeFileShouldHaveSameAmountOfRecordsAsDbaseFile(string resourceName, ExtractFileName entryFileName)
    {
        var sut = new ZipArchiveShapeFileReaderV1();

        using var beforeFcArchiveStream = new MemoryStream();
        await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.Resources.ZipArchiveShapeFileReaderTests.{resourceName}"))
        {
            await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
        }

        beforeFcArchiveStream.Position = 0;

        using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
        {
            var shpEntry = beforeFcArchive.Entries.Single(x => string.Equals(x.Name,  FeatureType.Extract.ToShapeFileName(entryFileName), StringComparison.InvariantCultureIgnoreCase));

            var dbfRecords = new SimpleExtractDbfReader<RoadNodeDbaseRecord>(RoadNodeDbaseRecord.Schema).Read(beforeFcArchive, entryFileName).Item1;
            Assert.True(dbfRecords.Any());

            var records = sut.Read(shpEntry).ToArray();
            Assert.Equal(dbfRecords.Count, records.Length);
        }
    }

    [Theory]
    [InlineData("eWegsegment.zip", ExtractFileName.Wegsegment)]
    public async Task RoadSegmentShapeFileShouldHaveSameAmountOfRecordsAsDbaseFile(string resourceName, ExtractFileName entryFileName)
    {
        var sut = new ZipArchiveShapeFileReaderV1();

        using var beforeFcArchiveStream = new MemoryStream();
        await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.Resources.ZipArchiveShapeFileReaderTests.{resourceName}"))
        {
            await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
        }

        beforeFcArchiveStream.Position = 0;

        using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
        {
            var shpEntry = beforeFcArchive.Entries.Single(x => string.Equals(x.Name, FeatureType.Extract.ToShapeFileName(entryFileName), StringComparison.InvariantCultureIgnoreCase));

            var dbfRecords = new SimpleExtractDbfReader<RoadSegmentDbaseRecord>(RoadSegmentDbaseRecord.Schema).Read(beforeFcArchive, entryFileName).Item1;
            Assert.True(dbfRecords.Any());

            var records = sut.Read(shpEntry).ToArray();
            Assert.Equal(dbfRecords.Count, records.Length);
        }
    }

    private class SimpleExtractDbfReader<TDbaseRecord> : ZipArchiveDbaseFeatureReader<TDbaseRecord, object>
        where TDbaseRecord : DbaseRecord, new()
    {
        public SimpleExtractDbfReader(DbaseSchema dbaseSchema)
            : base(Encoding.UTF8, dbaseSchema)
        {
        }

        protected override (object, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, TDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return (dbaseRecord, ZipArchiveProblems.None);
        }

        public (List<object>, ZipArchiveProblems) Read(ZipArchive archive, ExtractFileName fileName)
        {
            return Read(archive, FeatureType.Extract, fileName, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
        }
    }
}
