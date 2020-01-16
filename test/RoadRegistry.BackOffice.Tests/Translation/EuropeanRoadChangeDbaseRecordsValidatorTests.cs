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

    public class EuropeanRoadChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly EuropeanRoadChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord> _enumerator;

        public EuropeanRoadChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeEuropeanRoadNumber();
            _fixture.Customize<EuropeanRoadChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new EuropeanRoadChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        EU_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        EUNUMMER = {Value = _fixture.Create<EuropeanRoadNumber>().ToString()}
                    })
                    .OmitAutoProperties());

            _sut = new EuropeanRoadChangeDbaseRecordsValidator();
            _enumerator = new List<EuropeanRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("atteuropweg_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<EuropeanRoadChangeDbaseRecord>>(_sut);
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
                ZipArchiveProblems.Single(_entry.HasNoDbaseRecords(true)),
                result);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.EU_OIDN.Value = index + 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.EU_OIDN.Value = 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry
                        .AtDbaseRecord(new RecordNumber(2))
                        .IdentifierNotUnique(new AttributeId(1), new RecordNumber(1))
                ),
                result);
        }

        [Theory]
        [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
        public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
            Action<EuropeanRoadChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<EuropeanRoadChangeDbaseRecord>();
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
                    new Action<EuropeanRoadChangeDbaseRecord>(r => r.EU_OIDN.Value = null),
                    EuropeanRoadChangeDbaseRecord.Schema.EU_OIDN
                };

                yield return new object[]
                {
                    new Action<EuropeanRoadChangeDbaseRecord>(r => r.RECORDTYPE.Value = null),
                    EuropeanRoadChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<EuropeanRoadChangeDbaseRecord>(r => r.EUNUMMER.Value = null),
                    EuropeanRoadChangeDbaseRecord.Schema.EUNUMMER
                };

                yield return new object[]
                {
                    new Action<EuropeanRoadChangeDbaseRecord>(r => r.WS_OIDN.Value = null),
                    EuropeanRoadChangeDbaseRecord.Schema.WS_OIDN
                };
            }
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.EU_OIDN.Value = index + 1;
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
        public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.EU_OIDN.Value = 0;
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
        [InlineData("")]
        [InlineData("X40")]
        public void ValidateWithRecordsThatDoNotHaveEuropeanRoadNumbersReturnsExpectedResult(string number)
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.EU_OIDN.Value = index + 1;
                    record.EUNUMMER.Value = number;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Many(
                        _entry.AtDbaseRecord(new RecordNumber(1)).NotEuropeanRoadNumber(number),
                        _entry.AtDbaseRecord(new RecordNumber(2)).NotEuropeanRoadNumber(number)
                ),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveNullAsEuropeanRoadNumbersReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.EU_OIDN.Value = index + 1;
                    record.EUNUMMER.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Many(
                    _entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(EuropeanRoadChangeDbaseRecord.Schema.EUNUMMER),
                    _entry.AtDbaseRecord(new RecordNumber(2)).RequiredFieldIsNull(EuropeanRoadChangeDbaseRecord.Schema.EUNUMMER)
                ),
                result);
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord>(records, 1, exception);

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)
                ),
                result,
                new FileProblemComparer());
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidEuropeanRoadNumberReturnsExpectedResult()
        {
            var record = _fixture.Create<EuropeanRoadChangeDbaseRecord>();
            record.EUNUMMER.Value = "-1";
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NotEuropeanRoadNumber("-1")),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
        {
            var record = _fixture.Create<EuropeanRoadChangeDbaseRecord>();
            record.WS_OIDN.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
                result);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
