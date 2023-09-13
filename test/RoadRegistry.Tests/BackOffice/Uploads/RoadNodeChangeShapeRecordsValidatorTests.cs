namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Uploads;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeChangeShapeRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadNodeChangeShapeRecordValidator _sut;

    public RoadNodeChangeShapeRecordsValidatorTests()
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
        _sut = new RoadNodeChangeShapeRecordValidator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Update);
        _entry = _archive.CreateEntry("wegknoop_all.shp");
        _context = ZipArchiveValidationContext.Empty;
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveShapeRecordValidator()
    {
        Assert.IsAssignableFrom<IZipArchiveShapeRecordValidator>(_sut);
    }

    [Fact]
    public void ValidateContextCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, null));
    }

    [Fact]
    public void ValidateEntryCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveShapeEntryValidator(_sut).Validate(null, _context));
    }

    [Fact]
    public void ValidateWithInvalidGeometryTypeReturnsExpectedResult()
    {
        var stream = _fixture.CreateRoadSegmentShapeFileWithOneRecord();
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = _sut.Validate(_entry, RecordNumber.Initial, new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }), _context);

        var expected = ZipArchiveProblems.Single(_entry
            .AtShapeRecord(RecordNumber.Initial)
            .ShapeRecordShapeGeometryTypeMismatch(ShapeGeometryType.Point, nameof(LineString)));
        Assert.Equal(expected, result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithoutRecordsReturnsExpectedResult()
    {
        var stream = _fixture.CreateEmptyRoadNodeShapeFile();
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.HasNoShapeRecords()),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var shapeRecords = _fixture
            .CreateMany<ShapeRecord>(new Random().Next(1, 5))
            .Select((record, index) => (PointShapeContent)record.Content)
            .ToArray();

        var stream = _fixture.CreateRoadNodeShapeFile(shapeRecords);
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Same(_context, context);
    }
}
