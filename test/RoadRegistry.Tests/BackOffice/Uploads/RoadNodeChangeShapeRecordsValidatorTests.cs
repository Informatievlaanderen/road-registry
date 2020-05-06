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
    using Xunit;

    public class RoadNodeChangeShapeRecordsValidatorTests : IDisposable
    {
        private readonly RoadNodeChangeShapeRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IEnumerator<ShapeRecord> _enumerator;

        public RoadNodeChangeShapeRecordsValidatorTests()
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
            _fixture.Customize<RecordNumber>(customizer =>
                customizer.FromFactory(random => new RecordNumber(random.Next(1, int.MaxValue))));
            _fixture.Customize<ShapeRecord>(customization =>
                customization.FromFactory(random =>
                    new PointShapeContent(GeometryTranslator.FromGeometryPoint(_fixture.Create<NetTopologySuite.Geometries.Point>())).RecordAs(_fixture.Create<RecordNumber>())
                ).OmitAutoProperties()
            );
            _sut = new RoadNodeChangeShapeRecordsValidator();
            _enumerator = new List<ShapeRecord>().GetEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegknoop_all.shp");
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
                .Select((record, index) => record.Content.RecordAs(new RecordNumber(index + 1)))
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
                    _entry
                        .AtShapeRecord(RecordNumber.Initial)
                        .ShapeRecordShapeTypeMismatch(
                            ShapeType.Point,
                            ShapeType.NullShape)
                ),
                result);
        }

        [Fact(Skip = "It's impossible to mimic empty points at this time because they can no longer be serialized.")]
        public void ValidateWithEmptyPointRecordsReturnsExpectedResult()
        {
            var records = Enumerable.Range(0, 2)
                .Select(index =>
                    new PointShapeContent(GeometryTranslator.FromGeometryPoint(NetTopologySuite.Geometries.Point.Empty))
                        .RecordAs(new RecordNumber(index + 1)))
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
                ZipArchiveProblems.Single(
                    _entry.AtShapeRecord(new RecordNumber(2)).HasShapeRecordFormatError(exception)
                ),
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
