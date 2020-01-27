namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using Xunit;

    public class RoadSegmentChangeShapeRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentChangeShapeRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IEnumerator<ShapeRecord> _enumerator;

        public RoadSegmentChangeShapeRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<NetTopologySuite.Geometries.Point>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.Point(
                        _fixture.Create<double>(),
                        _fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<NetTopologySuite.Geometries.LineString>(customization =>
                customization.FromFactory(generator =>
                    new NetTopologySuite.Geometries.LineString(
                        new CoordinateArraySequence(
                            new []
                            {
                                new Coordinate(0.0,0.0),
                                new Coordinate(1.0,1.0),
                            }),
                        GeometryConfiguration.GeometryFactory
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(new[] {_fixture.Create<NetTopologySuite.Geometries.LineString>()})
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
        }

        [Fact]
        public void IsZipArchiveShapeRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveShapeRecordsValidator>(_sut);
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, _enumerator));
        }

        [Fact]
        public void ValidateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null));
        }

        [Fact]
        public void ValidateWithoutRecordsReturnsExpectedResult()
        {
            var result = _sut.Validate(_entry, _enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.HasNoShapeRecords()),
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
                                new LineString(new CoordinateArraySequence(new[]
                                {
                                    new Coordinate(index * 2.0, index * 2.0),
                                    new Coordinate(index * 2.0 + 1.0, index * 2.0 + 1.0)
                                }), GeometryConfiguration.GeometryFactory)
                            }))).RecordAs(new RecordNumber(index + 1)))
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
        }

        [Fact]
        public void ValidateWithNullRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<ShapeRecord>(new Random().Next(1, 5))
                .Select((record, index) => index == 0
                    ? NullShapeContent.Instance.RecordAs(new RecordNumber(index + 1))
                    : record.Content.RecordAs(new RecordNumber(index + 1)))
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtShapeRecord(RecordNumber.Initial).ShapeRecordShapeTypeMismatch(
                        ShapeType.PolyLineM,
                        ShapeType.NullShape)
                ),
                result);
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
                                            NetTopologySuite.Geometries.Point.Empty.Coordinate,
                                            NetTopologySuite.Geometries.Point.Empty.Coordinate
                                        }),
                                        GeometryConfiguration.GeometryFactory)
                                }))).RecordAs(new RecordNumber(index + 1)))
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Many(
                    _entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometryMismatch(),
                    _entry.AtShapeRecord(new RecordNumber(2)).ShapeRecordGeometryMismatch()
                ),
                result);
        }

        [Fact]
        public void ValidateWithTooLittleOrTooManyLinesPolyLineMRecordsReturnsExpectedResult()
        {
            var records = Enumerable.Range(0, 2)
                .Select(index =>
                {
                    if (index == 0)
                    {
                        return new PolyLineMShapeContent(
                            GeometryTranslator.FromGeometryMultiLineString(new MultiLineString(new LineString[0]))
                        ).RecordAs(new RecordNumber(index + 1));
                    }

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

            var result = _sut.Validate(_entry, records);

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
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 0.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 0.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 0.0, 15.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var multiLineString = new MultiLineString(
                new []
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

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometrySelfOverlaps()),
                result);
        }

        [Fact]
        public void ValidateWithSelfIntersectingRecordsReturnsExpectedResult()
        {
            var startPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(0.0, 10.0, 0.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint1 = new NetTopologySuite.Geometries.Point(new CoordinateM(10.0, 10.0, 10.0))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var middlePoint2 = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 20.0, 21.1803))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var endPoint = new NetTopologySuite.Geometries.Point(new CoordinateM(5.0, 0.0, 41.1803))
            {
                SRID = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
            };
            var lineString = new LineString(
                new CoordinateArraySequence(new[] { startPoint.Coordinate, middlePoint1.Coordinate, middlePoint2.Coordinate, endPoint.Coordinate }),
                GeometryConfiguration.GeometryFactory
            );
            var multiLineString = new MultiLineString(
                new []
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

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(1)).ShapeRecordGeometrySelfIntersects()),
                result);
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

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtShapeRecord(new RecordNumber(2)).HasShapeRecordFormatError(exception)),
                result,
                new FileProblemComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
