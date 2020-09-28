namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Schema;
    using Xunit;

    public class RoadSegmentSurfaceChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly RoadSegmentSurfaceChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadSegmentSurfaceChangeDbaseRecord> _enumerator;

        public RoadSegmentSurfaceChangeDbaseRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordType();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentGeometryDrawMethod();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeRoadSegmentMorphology();
            _fixture.CustomizeRoadSegmentStatus();
            _fixture.CustomizeRoadSegmentCategory();
            _fixture.CustomizeRoadSegmentAccessRestriction();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentSurfaceType();
            _fixture.CustomizeRoadSegmentPosition();
            _fixture.Customize<RoadSegmentSurfaceChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentSurfaceChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)_fixture.Create<RecordType>().Translation.Identifier},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        WV_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        VANPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TYPE = { Value = (short)_fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                    })
                    .OmitAutoProperties());

            _sut = new RoadSegmentSurfaceChangeDbaseRecordsTranslator();
            _enumerator = new List<RoadSegmentSurfaceChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attwegverharding_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<RoadSegmentSurfaceChangeDbaseRecord>>(_sut);
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
        public void TranslateWithRecordsForAddRoadSegmentReturnsExpectedResult()
        {
            var segment = _fixture.Create<Uploads.AddRoadSegment>();
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WV_OIDN.Value = index + 1;
                    record.WS_OIDN.Value = segment.TemporaryId;
                    record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();
            var changes = TranslatedChanges.Empty.Append(segment);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected =
                TranslatedChanges.Empty.Append(
                    records
                        .Where(record => record.RECORDTYPE.Value != (short)RecordType.Removed.Translation.Identifier)
                        .Aggregate(
                            segment,
                            (current, record) => current.WithSurface(
                                new Uploads.RoadSegmentSurfaceAttribute(
                                    new AttributeId(record.WV_OIDN.Value),
                                    RoadSegmentSurfaceType.ByIdentifier[record.TYPE.Value],
                                    new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value)),
                                    new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value)))
                            )
                        )
                );

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        [Fact]
        public void TranslateWithRecordsForModifyRoadSegmentReturnsExpectedResult()
        {
            var segment = _fixture.Create<Uploads.ModifyRoadSegment>();
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WV_OIDN.Value = index + 1;
                    record.WS_OIDN.Value = segment.Id;
                    record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();
            var changes = TranslatedChanges.Empty.Append(segment);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected =
                TranslatedChanges.Empty.Append(
                    records
                        .Where(record => record.RECORDTYPE.Value != (short)RecordType.Removed.Translation.Identifier)
                        .Aggregate(
                            segment,
                            (current, record) => current.WithSurface(
                                new Uploads.RoadSegmentSurfaceAttribute(
                                    new AttributeId(record.WV_OIDN.Value),
                                    RoadSegmentSurfaceType.ByIdentifier[record.TYPE.Value],
                                    new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value)),
                                    new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value)))
                            )
                        )
                );

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }


        [Fact]
        public void TranslateWithRecordsForRemoveRoadSegmentReturnsExpectedResult()
        {
            var segment = _fixture.Create<Uploads.RemoveRoadSegment>();
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WV_OIDN.Value = index + 1;
                    record.WS_OIDN.Value = segment.Id;
                    record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();
            var changes = TranslatedChanges.Empty.Append(segment);

            var result = _sut.Translate(_entry, enumerator, changes);

            var expected = TranslatedChanges.Empty.Append(segment);

            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
