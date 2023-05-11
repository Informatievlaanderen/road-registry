namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FeatureCompare;
using FeatureCompare.Translators;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using System.Text;
using System.IO;

public class FeatureReaderUnknownDbaseSchemaTests
{
    private static readonly Encoding Encoding = Encoding.UTF8;
    private const string FileName = "TEST";

    private readonly ExtractsZipArchiveTestData _testData;

    public FeatureReaderUnknownDbaseSchemaTests()
    {
        _testData = new ExtractsZipArchiveTestData();
    }

    private class TestDbaseSchema : DbaseSchema
    {
    }
    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestDbaseRecord : DbaseRecord
    {
        public static readonly TestDbaseSchema Schema = new();
    }

    [Fact]
    public void RoadSegment()
    {
        var reader = new RoadSegmentFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void RoadNode()
    {
        var reader = new RoadNodeFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void EuropeanRoad()
    {
        var reader = new EuropeanRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void NationalRoad()
    {
        var reader = new NationalRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void NumberedRoad()
    {
        var reader = new NumberedRoadFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void RoadSegmentLane()
    {
        var reader = new RoadSegmentLaneFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void RoadSegmentWidth()
    {
        var reader = new RoadSegmentWidthFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void RoadSegmentSurface()
    {
        var reader = new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void GradeSeparatedJunction()
    {
        var reader = new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }

    [Fact]
    public void TransactionZone()
    {
        var reader = new TransactionZoneFeatureCompareFeatureReader(Encoding);

        var zipArchive = CreateZipArchive();
        using (zipArchive)
        {
            Assert.Throws<DbaseReaderNotFoundException>(() => reader.Read(zipArchive.Entries, FeatureType.Levering, FileName));
        }
    }
    
    private ZipArchive CreateZipArchive()
    {
        var dbfStream = _testData.Fixture.CreateEmptyDbfFile<TestDbaseRecord>(TestDbaseRecord.Schema);

        var archiveStream = new MemoryStream();
        using (var createArchive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true, Encoding.UTF8))
        {
            dbfStream.Position = 0;
            using (var entryStream = createArchive.CreateEntry($"{FileName}.dbf").Open())
            {
                dbfStream.CopyTo(entryStream);
            }
        }
        archiveStream.Position = 0;

        return new ZipArchive(archiveStream, ZipArchiveMode.Read, false, Encoding.UTF8);
    }
}
