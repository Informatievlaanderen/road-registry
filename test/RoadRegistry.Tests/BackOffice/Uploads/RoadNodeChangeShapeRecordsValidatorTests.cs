namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Xunit;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeChangeShapeRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IEnumerator<ShapeRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadNodeChangeShapeRecordsValidator _sut;

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
        _sut = new RoadNodeChangeShapeRecordsValidator();
        _enumerator = new List<ShapeRecord>().GetEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("wegknoop_all.shp");
        _context = ZipArchiveValidationContext.Empty;
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveShapeRecordsValidator()
    {
        Assert.IsAssignableFrom<IZipArchiveShapeRecordsValidator>(_sut);
    }

    [Fact]
    public void ValidateEntryCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, _enumerator, _context));
    }

    [Fact]
    public void ValidateRecordsCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null, _context));
    }

    [Fact]
    public void ValidateContextCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, _enumerator, null));
    }

    [Fact]
    public void ValidateWithoutRecordsReturnsExpectedResult()
    {
        var (result, context) = _sut.Validate(_entry, _enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.HasNoShapeRecords()),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithValidRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<ShapeRecord>(new Random().Next(1, 5))
            .Select((record, index) => record.Content.RecordAs(new RecordNumber(index + 1)))
            .GetEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.None,
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithNullRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<ShapeRecord>(new Random().Next(1, 5))
            .Select((record, index) => index == 0
                ? NullShapeContent.Instance.RecordAs(new RecordNumber(1))
                : record.Content.RecordAs(new RecordNumber(index + 1)))
            .GetEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry
                    .AtShapeRecord(RecordNumber.Initial)
                    .ShapeRecordShapeTypeMismatch(
                        ShapeType.Point,
                        ShapeType.NullShape)
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact(Skip = "It's impossible to mimic empty points at this time because they can no longer be serialized.")]
    public void ValidateWithEmptyPointRecordsReturnsExpectedResult()
    {
        var records = Enumerable.Range(0, 2)
            .Select(index =>
                new PointShapeContent(GeometryTranslator.FromGeometryPoint(Point.Empty))
                    .RecordAs(new RecordNumber(index + 1)))
            .GetEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometryMismatch(),
                _entry.AtShapeRecord(new RecordNumber(2)).ShapeRecordGeometryMismatch()
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithProblematicRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<ShapeRecord>(new Random().Next(1, 5))
            .Select((record, index) => record.Content.RecordAs(new RecordNumber(index + 1)))
            .ToArray();
        var exception = new Exception("problem");
        var enumerator = new ProblematicShapeRecordEnumerator(records, 1, exception);

        var (result, context) = _sut.Validate(_entry, enumerator, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(
                _entry.AtShapeRecord(new RecordNumber(2)).HasShapeRecordFormatError(exception)
            ),
            result,
            new FileProblemComparer());
        Assert.Same(_context, context);
    }
}
