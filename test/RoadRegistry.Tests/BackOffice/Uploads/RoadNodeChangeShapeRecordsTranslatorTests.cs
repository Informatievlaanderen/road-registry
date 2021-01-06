namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Xunit;

    public class RoadNodeChangeShapeRecordsTranslatorTests : IDisposable
    {
        private readonly RoadNodeChangeShapeRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IEnumerator<ShapeRecord> _enumerator;

        public RoadNodeChangeShapeRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadNodeType();

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

            _sut = new RoadNodeChangeShapeRecordsTranslator();
            _enumerator = new List<ShapeRecord>().GetEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegknoop_all.shp");
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
        public void TranslateWithAddRecordsReturnsExpectedResult()
        {
            var node = _fixture.Create<AddRoadNode>();
            var record = _fixture.Create<ShapeRecord>().Content.RecordAs(node.RecordNumber);
            var records = new List<ShapeRecord> { record };
            var enumerator = records.GetEnumerator();
            var changes = TranslatedChanges.Empty.AppendChange(node);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected = TranslatedChanges.Empty.AppendChange(node.WithGeometry(GeometryTranslator.ToGeometryPoint(((PointShapeContent)record.Content).Shape)));

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        [Fact]
        public void TranslateWithModifyRecordsReturnsExpectedResult()
        {
            var node = _fixture.Create<ModifyRoadNode>();
            var record = _fixture.Create<ShapeRecord>().Content.RecordAs(node.RecordNumber);
            var records = new List<ShapeRecord> { record };
            var enumerator = records.GetEnumerator();
            var changes = TranslatedChanges.Empty.AppendChange(node);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected = TranslatedChanges.Empty.AppendChange(node.WithGeometry(GeometryTranslator.ToGeometryPoint(((PointShapeContent)record.Content).Shape)));

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        [Fact]
        public void TranslateWithRemoveRecordsReturnsExpectedResult()
        {
            var node = _fixture.Create<RemoveRoadNode>();
            var record = _fixture.Create<ShapeRecord>().Content.RecordAs(node.RecordNumber);
            var records = new List<ShapeRecord> { record };
            var enumerator = records.GetEnumerator();
            var changes = TranslatedChanges.Empty.AppendChange(node);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected = TranslatedChanges.Empty.AppendChange(node);

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
