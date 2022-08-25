namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice.Uploads;
using Xunit;
using Point = NetTopologySuite.Geometries.Point;

public class RoadSegmentChangeShapeRecordsValidatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveValidationContext _context;
    private readonly ZipArchiveEntry _entry;
    private readonly IEnumerator<ShapeRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadSegmentChangeShapeRecordsValidator _sut;

    public RoadSegmentChangeShapeRecordsValidatorTests()
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
        _fixture.Customize<LineString>(customization =>
            customization.FromFactory(generator =>
                new LineString(
                    new CoordinateArraySequence(
                        new[]
                        {
                            new Coordinate(0.0, 0.0),
                            new Coordinate(1.0, 1.0)
                        }),
                    GeometryConfiguration.GeometryFactory
                )
            ).OmitAutoProperties()
        );
        _fixture.Customize<MultiLineString>(customization =>
            customization.FromFactory(generator =>
                new MultiLineString(new[] { _fixture.Create<LineString>() })
            ).OmitAutoProperties()
        );
        _fixture.Customize<RecordNumber>(customizer =>
            customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));
        _fixture.Customize<ShapeRecord>(customization =>
            customization.FromFactory(random =>
                new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(_fixture.Create<MultiLineString>())).RecordAs(_fixture.Create<RecordNumber>())
            ).OmitAutoProperties()
        );
        _sut = new RoadSegmentChangeShapeRecordsValidator();
        _enumerator = new List<ShapeRecord>().GetEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("wegsegment_all.shp");
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
            .Select((record, index) => new PolyLineMShapeContent(
                GeometryTranslator.FromGeometryMultiLineString(
                    new MultiLineString(
                        new[]
                        {
                            new LineString(new CoordinateArraySequence(new[]
                            {
                                new Coordinate(index * 2.0, index * 2.0),
                                new Coordinate(index * 2.0 + 1.0, index * 2.0 + 1.0)
                            }), GeometryConfiguration.GeometryFactory)
                        }))).RecordAs(new RecordNumber(index + 1)))
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
                _entry.AtShapeRecord(RecordNumber.Initial).ShapeRecordShapeTypeMismatch(
                    ShapeType.PolyLineM,
                    ShapeType.NullShape)
            ),
            result);
        Assert.Same(_context, context);
    }

    [Fact(Skip = "It's impossible to mimic poly lines with empty points at this time because they can no longer be serialized.")]
    public void ValidateWithEmptyPolyLineMRecordsReturnsExpectedResult()
    {
        var records = Enumerable.Range(0, 2)
            .Select(index =>
                new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(
                        new MultiLineString(
                            new[]
                            {
                                new LineString(
                                    new CoordinateArraySequence(new[]
                                    {
                                        Point.Empty.Coordinate,
                                        Point.Empty.Coordinate
                                    }),
                                    GeometryConfiguration.GeometryFactory)
                            }))).RecordAs(new RecordNumber(index + 1)))
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
    public void ValidateWithTooLittleOrTooManyLinesPolyLineMRecordsReturnsExpectedResult()
    {
        var records = Enumerable.Range(0, 2)
            .Select(index =>
            {
                if (index == 0)
                    return new PolyLineMShapeContent(
                        GeometryTranslator.FromGeometryMultiLineString(new MultiLineString(Array.Empty<LineString>()))
                    ).RecordAs(new RecordNumber(1));

                return new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(
                        new MultiLineString(new[]
                        {
                            new LineString(
                                new CoordinateArraySequence(new[]
                                {
                                    new Coordinate(index * 2.0, index * 2.0),
                                    new Coordinate(index * 2.0 + 1.0, index * 2.0 + 1.0)
                                }),
                                GeometryConfiguration.GeometryFactory),
                            new LineString(
                                new CoordinateArraySequence(new[]
                                {
                                    new Coordinate(index * 4.0, index * 4.0),
                                    new Coordinate(index * 4.0 + 1.0, index * 4.0 + 1.0)
                                }),
                                GeometryConfiguration.GeometryFactory)
                        }))
                ).RecordAs(new RecordNumber(index + 1));
            })
            .GetEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometryLineCountMismatch(1, 0),
                _entry.AtShapeRecord(new RecordNumber(2)).ShapeRecordGeometryLineCountMismatch(1, 2)
            ),
            result);
    }

    [Fact]
    public void ValidateWithSelfOverlappingRecordsReturnsExpectedResult()
    {
        var startPoint = new Point(new CoordinateM(0.0, 0.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var middlePoint = new Point(new CoordinateM(10.0, 0.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint = new Point(new CoordinateM(5.0, 0.0, 15.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var multiLineString = new MultiLineString(
            new[]
            {
                new LineString(
                    new CoordinateArraySequence(new[] { startPoint.Coordinate, middlePoint.Coordinate, endPoint.Coordinate }),
                    GeometryConfiguration.GeometryFactory
                )
            })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var records = new List<ShapeRecord>
            {
                new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(multiLineString)).RecordAs(new RecordNumber(1))
            }
            .GetEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometrySelfOverlaps()),
            result);
        Assert.Same(_context, context);
    }

    [Fact]
    public void ValidateWithSelfIntersectingRecordsReturnsExpectedResult()
    {
        var startPoint = new Point(new CoordinateM(0.0, 10.0, 0.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var middlePoint1 = new Point(new CoordinateM(10.0, 10.0, 10.0))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var middlePoint2 = new Point(new CoordinateM(5.0, 20.0, 21.1803))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var endPoint = new Point(new CoordinateM(5.0, 0.0, 41.1803))
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var lineString = new LineString(
            new CoordinateArraySequence(new[] { startPoint.Coordinate, middlePoint1.Coordinate, middlePoint2.Coordinate, endPoint.Coordinate }),
            GeometryConfiguration.GeometryFactory
        );
        var multiLineString = new MultiLineString(
            new[]
            {
                lineString
            })
        {
            SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
        };
        var records = new List<ShapeRecord>
            {
                new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(multiLineString)).RecordAs(new RecordNumber(1))
            }
            .GetEnumerator();

        var (result, context) = _sut.Validate(_entry, records, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometrySelfIntersects()),
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
            ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(2)).HasShapeRecordFormatError(exception)),
            result,
            new FileProblemComparer());
        Assert.Same(_context, context);
    }
}
