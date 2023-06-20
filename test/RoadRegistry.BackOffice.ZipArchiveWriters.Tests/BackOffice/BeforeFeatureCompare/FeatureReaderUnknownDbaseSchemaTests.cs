namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using FeatureCompare.Translators;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice;

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

        var archiveStream = new MemoryStream();
        using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            dbfStream.Position = 0;
            using (var entryStream = createArchive.CreateEntry(FeatureType.Change.GetDbfFileName(fileName)).Open())
            {
                dbfStream.CopyTo(entryStream);
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
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void GradeSeparatedJunction()
    {
        var reader = new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void NationalRoad()
    {
        var reader = new NationalRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void NumberedRoad()
    {
        var reader = new NumberedRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void RoadNode()
    {
        var reader = new RoadNodeFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void RoadSegment()
    {
        var reader = new RoadSegmentFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void RoadSegmentLane()
    {
        var reader = new RoadSegmentLaneFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void RoadSegmentSurface()
    {
        var reader = new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void RoadSegmentWidth()
    {
        var reader = new RoadSegmentWidthFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Transactiezones);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Transactiezones));
        }
    }

    [Fact]
    public void TransactionZone()
    {
        var reader = new TransactionZoneFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive(ExtractFileName.Wegsegment);
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Change, ExtractFileName.Wegsegment));
        }
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
