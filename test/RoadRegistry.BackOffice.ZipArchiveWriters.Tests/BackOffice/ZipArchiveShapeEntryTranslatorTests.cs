namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using Uploads;
using Point = NetTopologySuite.Geometries.Point;

public class ZipArchiveShapeEntryTranslatorTests
{
    private readonly Fixture _fixture;

    public ZipArchiveShapeEntryTranslatorTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Point>(customization =>
            customization.FromFactory(generator =>
                new Point(
                    _fixture.Create<double>(),
                    _fixture.Create<double>()
                )
            ).OmitAutoProperties()
        );
        _fixture.Customize<RecordNumber>(customizer =>
            customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));
        _fixture.Customize<ShapeRecord>(customization =>
            customization.FromFactory(random =>
                new PointShapeContent(GeometryTranslator.FromGeometryPoint(_fixture.Create<Point>())).RecordAs(_fixture.Create<RecordNumber>())
            ).OmitAutoProperties()
        );
    }

    [Fact]
    public void EncodingCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ZipArchiveShapeEntryTranslator(
                null,
                new FakeShapeRecordTranslator()));
    }

    [Fact]
    public void TranslateChangesCanNotBeNull()
    {
        var sut = new ZipArchiveShapeEntryTranslator(
            Encoding.Default,
            new FakeShapeRecordTranslator());

        using (var stream = new MemoryStream())
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("entry");
            Assert.Throws<ArgumentNullException>(() => sut.Translate(entry, null));
        }
    }

    [Fact]
    public void TranslateEntryCanNotBeNull()
    {
        var sut = new ZipArchiveShapeEntryTranslator(
            Encoding.Default,
            new FakeShapeRecordTranslator());

        Assert.Throws<ArgumentNullException>(() => sut.Translate(null, TranslatedChanges.Empty));
    }

    [Fact]
    public void TranslatePassesExpectedShapeRecordsToShapeRecordTranslator()
    {
        var translator = new CollectShapeRecordTranslator();
        var sut = new ZipArchiveShapeEntryTranslator(Encoding.UTF8, translator);
        var records = _fixture.CreateMany<ShapeRecord>(2).ToArray();
        var fileSize = records.Aggregate(ShapeFileHeader.Length, (length, record) => length.Plus(record.Length));
        var header = new ShapeFileHeader(
            fileSize,
            ShapeType.Point,
            BoundingBox3D.Empty);

        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("entry");
                using (var entryStream = entry.Open())
                using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                {
                    header.Write(writer);
                    foreach (var record in records)
                    {
                        record.Write(writer);
                    }

                    entryStream.Flush();
                }
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = archive.GetEntry("entry");

                var result = sut.Translate(entry, TranslatedChanges.Empty);

                Assert.Equal(TranslatedChanges.Empty, result, new TranslatedChangeEqualityComparer());
                Assert.Equal(records, translator.Collected, new ShapeRecordEqualityComparer());
            }
        }
    }

    [Fact]
    public void TranslateReturnsExpectedResultWhenShapeRecordTranslatorReturnsChanges()
    {
        var changes = TranslatedChanges.Empty
            .AppendChange(new AddRoadNode(new RecordNumber(1), new RoadNodeId(1), RoadNodeType.RealNode))
            .AppendChange(new AddRoadNode(new RecordNumber(1), new RoadNodeId(1), RoadNodeType.RealNode));
        var sut = new ZipArchiveShapeEntryTranslator(
            Encoding.UTF8,
            new FakeShapeRecordTranslator(ignored => changes));
        var header = new ShapeFileHeader(
            ShapeFileHeader.Length,
            ShapeType.Point,
            BoundingBox3D.Empty);

        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("entry");
                using (var entryStream = entry.Open())
                using (var writer = new BinaryWriter(entryStream, Encoding.UTF8))
                {
                    header.Write(writer);
                    entryStream.Flush();
                }
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = archive.GetEntry("entry");

                var result = sut.Translate(entry, TranslatedChanges.Empty);

                Assert.Equal(
                    changes,
                    result,
                    new TranslatedChangeEqualityComparer());
            }
        }
    }

    [Fact]
    public void TranslatorCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ZipArchiveShapeEntryTranslator(
                Encoding.Default,
                null));
    }

    private class CollectShapeRecordTranslator : IZipArchiveShapeRecordsTranslator
    {
        public ShapeRecord[] Collected { get; private set; }

        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, TranslatedChanges changes)
        {
            var collected = new List<ShapeRecord>();
            while (records.MoveNext())
            {
                collected.Add(records.Current);
            }

            Collected = collected.ToArray();

            return changes;
        }
    }

    private class FakeShapeRecordTranslator : IZipArchiveShapeRecordsTranslator
    {
        private readonly Func<TranslatedChanges, TranslatedChanges> _translation;

        public FakeShapeRecordTranslator(Func<TranslatedChanges, TranslatedChanges> translation = null)
        {
            _translation = translation ?? (changes => changes);
        }

        public TranslatedChanges Translate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records, TranslatedChanges changes)
        {
            return _translation(changes);
        }
    }
}