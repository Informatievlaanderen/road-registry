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

    public class NationalRoadChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly NationalRoadChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> _enumerator;

        public NationalRoadChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeNationalRoadNumber();
            _fixture.Customize<NationalRoadChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new NationalRoadChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        NW_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        IDENT2 = {Value = _fixture.Create<NationalRoadNumber>().ToString()}
                    })
                    .OmitAutoProperties());

            _sut = new NationalRoadChangeDbaseRecordsValidator();
            _enumerator = new List<NationalRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attnationweg_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<NationalRoadChangeDbaseRecord>>(_sut);
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
            var result = _sut.Validate(_entry,  _enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.HasNoDbaseRecords(true)),
                result);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<NationalRoadChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.NW_OIDN.Value = index + 1;
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
                .CreateMany<NationalRoadChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.NW_OIDN.Value = index + 1;
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
                .CreateMany<NationalRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.NW_OIDN.Value = 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2))
                        .IdentifierNotUnique(
                            new AttributeId(1),
                            new RecordNumber(1))
                    ),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<NationalRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.NW_OIDN.Value = 0;
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
            Action<NationalRoadChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<NationalRoadChangeDbaseRecord>();
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
                    new Action<NationalRoadChangeDbaseRecord>(r => r.NW_OIDN.Reset()),
                    NationalRoadChangeDbaseRecord.Schema.NW_OIDN
                };

                yield return new object[]
                {
                    new Action<NationalRoadChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                    NationalRoadChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<NationalRoadChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                    NationalRoadChangeDbaseRecord.Schema.WS_OIDN
                };

                yield return new object[]
                {
                    new Action<NationalRoadChangeDbaseRecord>(r => r.IDENT2.Reset()),
                    NationalRoadChangeDbaseRecord.Schema.IDENT2
                };
            }
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<NationalRoadChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<NationalRoadChangeDbaseRecord>(records, 1, exception);

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
                result,
                new FileProblemComparer());
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
        {
            var record = _fixture.Create<NationalRoadChangeDbaseRecord>();
            record.WS_OIDN.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidNationalRoadNumberReturnsExpectedResult()
        {
            var record = _fixture.Create<NationalRoadChangeDbaseRecord>();
            record.IDENT2.Value = "-1";
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NotNationalRoadNumber("-1")),
                result);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
