namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using Xunit;

    public class RoadSegmentChangeShapeRecordsTranslatorTests : IDisposable
    {
        private readonly RoadSegmentChangeShapeRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IEnumerator<ShapeRecord> _enumerator;

        public RoadSegmentChangeShapeRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentGeometryDrawMethod();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeRoadSegmentMorphology();
            _fixture.CustomizeRoadSegmentStatus();
            _fixture.CustomizeRoadSegmentCategory();
            _fixture.CustomizeRoadSegmentAccessRestriction();

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

            _sut = new RoadSegmentChangeShapeRecordsTranslator();
            _enumerator = new List<ShapeRecord>().GetEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegsegment_all.shp");
        }

        [Fact]
        public void IsZipArchiveShapeRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveShapeRecordsTranslator>(_sut);
        }

        [Fact]
        public void TranslateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Translate(null, _enumerator, TranslatedChanges.Empty));
        }

        [Fact]
        public void TranslateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, null, TranslatedChanges.Empty));
        }

        [Fact]
        public void TranslateChangesCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, _enumerator, null));
        }

        [Fact]
        public void TranslateWithoutRecordsReturnsExpectedResult()
        {
            var result = _sut.Translate(_entry, _enumerator, TranslatedChanges.Empty);

            Assert.Equal(
                TranslatedChanges.Empty,
                result);
        }

        [Fact]
        public void TranslateWithRecordsReturnsExpectedResult()
        {
            var segment = _fixture.Create<AddRoadSegment>();
            var record = _fixture.Create<ShapeRecord>().Content.RecordAs(segment.RecordNumber);
            var records = new List<ShapeRecord> { record };
            var enumerator = records.GetEnumerator();
            var changes = TranslatedChanges.Empty.Append(segment);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected = TranslatedChanges.Empty.Append(
                segment.WithGeometry(
                GeometryTranslator.ToGeometryMultiLineString(
                    ((PolyLineMShapeContent) record.Content).Shape)
                )
            );

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
