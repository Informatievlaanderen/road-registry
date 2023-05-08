namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice;

using System.IO.Compression;
using System.Text;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using NetTopologySuite.Geometries;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using Uploads;
using Point = NetTopologySuite.Geometries.Point;

public class ZipArchiveShapeEntryValidatorTests
{
    private readonly ZipArchiveValidationContext _context;
    private readonly Fixture _fixture;

    public ZipArchiveShapeEntryValidatorTests()
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
        _context = ZipArchiveValidationContext.Empty;
    }
    
    [Fact]
    public void ValidateContextCanNotBeNull()
    {
        var sut = new ZipArchiveShapeEntryValidator(new FakeShapeRecordValidator());
        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntry("entry");
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                Assert.Throws<ArgumentNullException>(() => sut.Validate(archive.GetEntry("entry"), null));
            }
        }
    }

    [Fact]
    public void ValidateEntryCanNotBeNull()
    {
        var sut = new ZipArchiveShapeEntryValidator(
            new FakeShapeRecordValidator());

        Assert.Throws<ArgumentNullException>(() => sut.Validate(null, _context));
    }

    [Fact]
    public void ValidatePassesExpectedShapeRecordsToShapeRecordValidator()
    {
        var validator = new CollectShapeRecordValidator();
        var sut = new ZipArchiveShapeEntryValidator(validator);
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
                    foreach (var record in records) record.Write(writer);
                    
                    entryStream.Flush();
                }
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = archive.GetEntry("entry");

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(ZipArchiveProblems.None, result);
                Assert.Equal(records, validator.Collected, new ShapeRecordEqualityComparer());
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultWhenEntryStreamContainsMalformedShapeHeader()
    {
        var sut = new ZipArchiveShapeEntryValidator(
            new FakeShapeRecordValidator());

        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry("entry");
                using (var entryStream = entry.Open())
                {
                    entryStream.WriteByte(0);
                    entryStream.WriteByte(0);
                    entryStream.WriteByte(0);
                    entryStream.WriteByte(0);
                    entryStream.Flush();
                }
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = archive.GetEntry("entry");

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.Single(entry.HasShapeHeaderFormatError(
                        new ShapeFileHeaderException("The File Code field does not match 9994."))
                    ),
                    result,
                    new FileProblemComparer());
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultWhenEntryStreamIsEmpty()
    {
        var sut = new ZipArchiveShapeEntryValidator(
            new FakeShapeRecordValidator());

        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntry("entry");
            }

            stream.Flush();
            stream.Position = 0;

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = archive.GetEntry("entry");

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.Single(entry.HasShapeHeaderFormatError(
                        new EndOfStreamException("Unable to read beyond the end of the stream."))
                    ),
                    result,
                    new FileProblemComparer());
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidateReturnsExpectedResultWhenShapeRecordValidatorReturnsErrors()
    {
        var problems = new FileProblem[]
        {
            new FileError("file1", "error1", new ProblemParameter("parameter1", "value1")),
            new FileWarning("file2", "error2", new ProblemParameter("parameter2", "value2"))
        };
        var sut = new ZipArchiveShapeEntryValidator(
            new FakeShapeRecordValidator(problems));
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

                var (result, context) = sut.Validate(entry, _context);

                Assert.Equal(
                    ZipArchiveProblems.None.AddRange(problems),
                    result);
                Assert.Same(_context, context);
            }
        }
    }

    [Fact]
    public void ValidatorCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ZipArchiveShapeEntryValidator(
                null));
    }

    private class CollectShapeRecordValidator : IZipArchiveShapeRecordValidator
    {
        public List<ShapeRecord> Collected { get; } = new();
        
        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, RecordNumber recordNumber, Geometry geometry, ZipArchiveValidationContext context)
        {
            Collected.Add(new PointShapeContent(GeometryTranslator.FromGeometryPoint((Point)geometry)).RecordAs(recordNumber));
            return (ZipArchiveProblems.None, context);
        }
    }

    private class FakeShapeRecordValidator : IZipArchiveShapeRecordValidator
    {
        private readonly FileProblem[] _problems;

        public FakeShapeRecordValidator(params FileProblem[] problems)
        {
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }
        
        public (ZipArchiveProblems, ZipArchiveValidationContext) Validate(ZipArchiveEntry entry, RecordNumber recordNumber, Geometry geometry, ZipArchiveValidationContext context)
        {
            return (ZipArchiveProblems.None.AddRange(_problems), context);
        }
    }
}
