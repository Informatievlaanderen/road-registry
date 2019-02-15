namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class RoadSegmentChangeShapeRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentChangeShapeRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private IEnumerator<ShapeRecord> _enumerator;

        public RoadSegmentChangeShapeRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<PointM>(customization =>
                customization.FromFactory(generator =>
                    new PointM(
                        _fixture.Create<double>(),
                        _fixture.Create<double>(),
                        _fixture.Create<double>(),
                        _fixture.Create<double>()
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<ILineString>(customization =>
                customization.FromFactory(generator =>
                    new LineString(
                        new PointSequence(
                            new []
                            {
                                new PointM(0.0,0.0),
                                new PointM(1.0,1.0)
                            }),
                        GeometryConfiguration.GeometryFactory
                    )
                ).OmitAutoProperties()
            );
            _fixture.Customize<MultiLineString>(customization =>
                customization.FromFactory(generator =>
                    new MultiLineString(new[] {_fixture.Create<ILineString>()})
                ).OmitAutoProperties()
            );
            _fixture.Customize<RecordNumber>(customizer =>
                customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));
            _fixture.Customize<ShapeRecord>(customization =>
                customization.FromFactory(random =>
                    new PolyLineMShapeContent(_fixture.Create<MultiLineString>()).RecordAs(_fixture.Create<RecordNumber>())
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
                ZipArchiveErrors.None.NoShapeRecords(_entry.Name),
                result);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<ShapeRecord>(new Random().Next(1, 5))
                .Select((record, index) => new PolyLineMShapeContent(
                    new MultiLineString(
                        new ILineString[]
                        {
                            new LineString(new PointSequence(new []
                            {
                                new PointM(index * 2.0, index * 2.0),
                                new PointM(index * 2.0 + 1.0, index * 2.0 +1.0)
                            }),GeometryConfiguration.GeometryFactory)
                        })).RecordAs(new RecordNumber(index + 1)))
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None,
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
                ZipArchiveErrors.None.ShapeRecordShapeTypeMismatch(
                    _entry.Name,
                    RecordNumber.Initial,
                    ShapeType.PolyLineM,
                    ShapeType.NullShape),
                result);
        }

        [Fact]
        public void ValidateWithEmptyPolyLineMRecordsReturnsExpectedResult()
        {
            var records = Enumerable.Range(0, 2)
                .Select(index =>
                    new PolyLineMShapeContent(
                        new MultiLineString(
                            new ILineString[]
                            {
                                new LineString(
                                    new PointSequence(new []
                                    {
                                        new PointM(new PointSequence(new PointM[0])),
                                        new PointM(new PointSequence(new PointM[0]))
                                    }),
                                    GeometryConfiguration.GeometryFactory)
                            })).RecordAs(new RecordNumber(index + 1)))
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None
                    .ShapeRecordGeometryMismatch(_entry.Name, new RecordNumber(1))
                    .ShapeRecordGeometryMismatch(_entry.Name, new RecordNumber(2)),
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
                            new MultiLineString(new ILineString[0])
                        ).RecordAs(new RecordNumber(index + 1));
                    }

                    return new PolyLineMShapeContent(
                        new MultiLineString(new ILineString[]
                        {
                            new LineString(
                                new PointSequence(new[]
                                {
                                    new PointM(index * 2.0, index * 2.0),
                                    new PointM(index * 2.0 + 1.0, index * 2.0 + 1.0)
                                }),
                                GeometryConfiguration.GeometryFactory),
                            new LineString(
                                new PointSequence(new[]
                                {
                                    new PointM(index * 4.0, index * 4.0),
                                    new PointM(index * 4.0 + 1.0, index * 4.0 + 1.0)
                                }),
                                GeometryConfiguration.GeometryFactory)
                        })
                    ).RecordAs(new RecordNumber(index + 1));
                })
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None
                    .ShapeRecordGeometryLineCountMismatch(_entry.Name, new RecordNumber(1), 1, 0)
                    .ShapeRecordGeometryLineCountMismatch(_entry.Name, new RecordNumber(2), 1, 2),
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
                ZipArchiveErrors
                    .None
                    .ShapeRecordFormatError(_entry.Name, new RecordNumber(2), exception),
                result,
                new ErrorComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
