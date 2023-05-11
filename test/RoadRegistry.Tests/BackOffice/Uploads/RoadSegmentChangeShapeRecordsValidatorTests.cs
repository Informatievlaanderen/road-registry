namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
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
    private readonly RoadSegmentChangeShapeRecordValidator _sut;

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
                        new Coordinate[]
                        {
                            new CoordinateM(0.0, 0.0, 0),
                            new CoordinateM(1.0, 1.0, 0)
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
        _sut = new RoadSegmentChangeShapeRecordValidator();
        _enumerator = new List<ShapeRecord>().GetEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Update);
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
    public void ValidateWithoutRecordsReturnsExpectedResult()
    {
        var stream = _fixture.CreateEmptyRoadSegmentShapeFile();
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
    public void ValidateWithInvalidGeometryTypeReturnsExpectedResult()
    {
        var stream = _fixture.CreateRoadNodeShapeFileWithOneRecord();
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = _sut.Validate(_entry, RecordNumber.Initial, new Point(0, 0), _context);

        var expected = ZipArchiveProblems.Single(_entry
            .AtShapeRecord(RecordNumber.Initial)
            .ShapeRecordShapeGeometryTypeMismatch(ShapeGeometryType.LineStringM, nameof(Point)));
        Assert.Equal(expected, result);
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
        var records = new []
            {
                new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(multiLineString))
            };

        var stream = _fixture.CreateRoadSegmentShapeFile(records);
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometrySelfIntersects()),
            result);
        Assert.Same(_context, context);
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
        var records = new []
        {
            new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(multiLineString))
        };

        var stream = _fixture.CreateRoadSegmentShapeFile(records);
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometrySelfOverlaps()),
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
                    );

                return new PolyLineMShapeContent(
                    GeometryTranslator.FromGeometryMultiLineString(
                        new MultiLineString(new[]
                        {
                            new LineString(
                                new CoordinateArraySequence(new Coordinate[]
                                {
                                    new CoordinateM(index * 2.0, index * 2.0),
                                    new CoordinateM(index * 2.0 + 1.0, index * 2.0 + 1.0)
                                }),
                                GeometryConfiguration.GeometryFactory),
                            new LineString(
                                new CoordinateArraySequence(new Coordinate[]
                                {
                                    new CoordinateM(index * 4.0, index * 4.0, 0),
                                    new CoordinateM(index * 4.0 + 1.0, index * 4.0 + 1.0, 0)
                                }),
                                GeometryConfiguration.GeometryFactory)
                        }))
                );
            })
            .ToArray();

        var stream = _fixture.CreateRoadSegmentShapeFile(records);
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, _context);

        Assert.Equal(
            ZipArchiveProblems.Many(
                _entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometryLineCountMismatch(1, 0),
                _entry.AtShapeRecord(new RecordNumber(2)).ShapeRecordGeometryLineCountMismatch(1, 2)
            ),
            result);
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
                            new LineString(new CoordinateArraySequence(new Coordinate[]
                            {
                                new CoordinateM(index * 2.0, index * 2.0, 0),
                                new CoordinateM(index * 2.0 + 1.0, index * 2.0 + 1.0, 0)
                            }), GeometryConfiguration.GeometryFactory)
                        }))))
            .ToArray();

        var stream = _fixture.CreateRoadSegmentShapeFile(records);
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

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ValidateWithGeometryHasInvalidMeasureOrdinatesReturnsExpectedResult(double m)
    {
        var multiLineString = new MultiLineString(
            new[]
            {
                new LineString(new CoordinateArraySequence(new[]
                {
                    new CoordinateM(0, 0, m),
                    new CoordinateM(1,1,0)
                }), GeometryConfiguration.GeometryFactory)
            });

        var records = new []
            {
                new PolyLineMShapeContent(GeometryTranslator.FromGeometryMultiLineString(multiLineString))
            };

        var stream = _fixture.CreateRoadSegmentShapeFile(records);
        stream.Position = 0;
        using (var entryStream = _entry.Open())
        {
            stream.CopyTo(entryStream);
        }

        var (result, context) = new ZipArchiveShapeEntryValidator(_sut).Validate(_entry, _context);

        Assert.Equal(
            ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometryHasInvalidMeasureOrdinates()),
            result);
        Assert.Same(_context, context);
    }
}
