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

    public class RoadSegmentLaneChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentLaneChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord> _enumerator;

        public RoadSegmentLaneChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentLaneCount();
            _fixture.CustomizeRoadSegmentLaneDirection();
            _fixture.CustomizeRoadSegmentPosition();
            _fixture.Customize<RoadSegmentLaneChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentLaneChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        RS_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        VANPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        AANTAL = { Value = (short)_fixture.Create<RoadSegmentLaneCount>().ToInt32() },
                        RICHTING = {Value = (short) _fixture.Create<RoadSegmentLaneDirection>().Translation.Identifier}
                    })
                    .OmitAutoProperties());

            _sut = new RoadSegmentLaneChangeDbaseRecordsValidator();
            _enumerator = new List<RoadSegmentLaneChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attrijstroken_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentLaneChangeDbaseRecord>>(_sut);
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
                ZipArchiveProblems.Single(_entry.HasNoDbaseRecords(false)),
                result);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentLaneChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.RS_OIDN.Value = index + 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.RS_OIDN.Value = index + 1;
                    record.RECORDTYPE.Value = -1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Many(
                    _entry
                        .AtDbaseRecord(new RecordNumber(1))
                        .RecordTypeMismatch(-1),
                    _entry
                        .AtDbaseRecord(new RecordNumber(2))
                        .RecordTypeMismatch(-1)
                ),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.RS_OIDN.Value = 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
                        new AttributeId(1),
                        new RecordNumber(1))
                ),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.RS_OIDN.Value = 0;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Many(
                    _entry.AtDbaseRecord(new RecordNumber(1)).IdentifierZero(),
                    _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierZero()
                ),
                result);
        }

        [Theory]
        [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
        public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
            Action<RoadSegmentLaneChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<RoadSegmentLaneChangeDbaseRecord>();
            modifier(record);
            var records = new[] {record}.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Contains(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(field), result);
        }

        public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
        {
            get
            {
                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.WS_OIDN.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.WS_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.RECORDTYPE.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.RS_OIDN.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.RS_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.AANTAL.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.AANTAL
                };

                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.RICHTING.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.RICHTING
                };

                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.VANPOSITIE.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.VANPOSITIE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentLaneChangeDbaseRecord>(r => r.TOTPOSITIE.Value = null),
                    RoadSegmentLaneChangeDbaseRecord.Schema.TOTPOSITIE
                };
            }
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentLaneChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentLaneChangeDbaseRecord>(records, 1, exception);

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)
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
