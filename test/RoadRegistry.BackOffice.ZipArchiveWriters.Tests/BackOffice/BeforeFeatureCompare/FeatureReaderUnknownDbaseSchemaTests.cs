namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using FeatureCompare.Translators;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice;
using Uploads;

public class FeatureReaderUnknownDbaseSchemaTests
{
    private static readonly Encoding Encoding = Encoding.UTF8;
    private readonly ExtractsZipArchiveTestData _testData;

    public FeatureReaderUnknownDbaseSchemaTests()
    {
        _testData = new ExtractsZipArchiveTestData();
    }

    private ZipArchive CreateZipArchive(ExtractFileName fileName)
    {
        var dbfStream = _testData.Fixture.CreateEmptyDbfFile<TestDbaseRecord>(TestDbaseRecord.Schema);
        var shpStream = _testData.Fixture.CreateEmptyRoadNodeShapeFile();
        var prjStream = _testData.Fixture.CreateProjectionFormatFileWithOneRecord();

        var archiveStream = new MemoryStream();
        using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            dbfStream.Position = 0;
            using (var entryStream = createArchive.CreateEntry(FeatureType.Change.GetDbaseFileName(fileName)).Open())
            {
                dbfStream.CopyTo(entryStream);
            }

            shpStream.Position = 0;
            using (var entryStream = createArchive.CreateEntry(FeatureType.Change.GetShapeFileName(fileName)).Open())
            {
                shpStream.CopyTo(entryStream);
            }

            prjStream.Position = 0;
            using (var entryStream = createArchive.CreateEntry(FeatureType.Change.GetProjectionFileName(fileName)).Open())
            {
                prjStream.CopyTo(entryStream);
            }
        }

        archiveStream.Position = 0;

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
    }

    [Fact]
    public void EuropeanRoad()
    {
        var reader = new EuropeanRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void GradeSeparatedJunction()
    {
        var reader = new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void NationalRoad()
    {
        var reader = new NationalRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void NumberedRoad()
    {
        var reader = new NumberedRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void RoadNode()
    {
        var reader = new RoadNodeFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void RoadSegment()
    {
        var reader = new RoadSegmentFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void RoadSegmentLane()
    {
        var reader = new RoadSegmentLaneFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void RoadSegmentSurface()
    {
        var reader = new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void RoadSegmentWidth()
    {
        var reader = new RoadSegmentWidthFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    [Fact]
    public void TransactionZone()
    {
        var reader = new TransactionZoneFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Wegsegment);
        using (zipArchive)
        {
            var problems = reader.Read(zipArchive, FeatureType.Change, ExtractFileName.Wegsegment, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty)).Item2;
            Assert.True(AllDbfFilesHaveDbaseSchemaMismatch(problems));
        }
    }

    private static bool AllDbfFilesHaveDbaseSchemaMismatch(ZipArchiveProblems problems)
    {
        return problems.ToList().TrueForAll(x => !x.File.EndsWith(".DBF", StringComparison.InvariantCultureIgnoreCase)
                                                 || x.Reason == nameof(DbaseFileProblems.HasDbaseSchemaMismatch));
    }

    private class TestDbaseSchema : DbaseSchema
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestDbaseRecord : DbaseRecord
    {
        public static readonly TestDbaseSchema Schema = new();
    }
}
