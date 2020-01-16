namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using Xunit;

    public class RoadSegmentWidthChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly RoadSegmentWidthChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadSegmentWidthChangeDbaseRecord> _enumerator;

        public RoadSegmentWidthChangeDbaseRecordsTranslatorTests()
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
            _fixture.CustomizeRoadSegmentWidth();
            _fixture.CustomizeRoadSegmentPosition();
            _fixture.Customize<RoadSegmentWidthChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentWidthChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)_fixture.Create<RecordType>().Translation.Identifier},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        WB_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        VANPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        BREEDTE = { Value = (short)_fixture.Create<RoadSegmentWidth>().ToInt32() }
                    })
                    .OmitAutoProperties());


            _sut = new RoadSegmentWidthChangeDbaseRecordsTranslator();
            _enumerator = new List<RoadSegmentWidthChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attwegbreedte_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<RoadSegmentWidthChangeDbaseRecord>>(_sut);
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
            var records = _fixture
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WB_OIDN.Value = index + 1;
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
                            (current, record) => current.WithWidth(
                                new RoadSegmentWidthAttribute(
                                    new AttributeId(record.WB_OIDN.Value.GetValueOrDefault()),
                                    new RoadSegmentWidth(record.BREEDTE.Value.GetValueOrDefault()),
                                    new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value.GetValueOrDefault())),
                                    new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value.GetValueOrDefault())))
                            )
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
