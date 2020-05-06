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

    public class NumberedRoadChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly NumberedRoadChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> _enumerator;

        public NumberedRoadChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeNumberedRoadNumber();
            _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            _fixture.CustomizeRoadSegmentNumberedRoadDirection();
            _fixture.Customize<NumberedRoadChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new NumberedRoadChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        GW_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        IDENT8 = {Value = _fixture.Create<NumberedRoadNumber>().ToString()},
                        RICHTING = {Value = (short) _fixture.Create<RoadSegmentNumberedRoadDirection>().Translation.Identifier},
                        VOLGNUMMER = {Value = _fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32()}
                    })
                    .OmitAutoProperties());

            _sut = new NumberedRoadChangeDbaseRecordsValidator();
            _enumerator = new List<NumberedRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attgenumweg_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<NumberedRoadChangeDbaseRecord>>(_sut);
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
                .CreateMany<NumberedRoadChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.GW_OIDN.Value = index + 1;
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
                .CreateMany<NumberedRoadChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.GW_OIDN.Value = index + 1;
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
                .CreateMany<NumberedRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.GW_OIDN.Value = 1;
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
                .CreateMany<NumberedRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.GW_OIDN.Value = 0;
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
            Action<NumberedRoadChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
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
                    new Action<NumberedRoadChangeDbaseRecord>(r => r.GW_OIDN.Reset()),
                    NumberedRoadChangeDbaseRecord.Schema.GW_OIDN
                };

                yield return new object[]
                {
                    new Action<NumberedRoadChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                    NumberedRoadChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<NumberedRoadChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                    NumberedRoadChangeDbaseRecord.Schema.WS_OIDN
                };

                yield return new object[]
                {
                    new Action<NumberedRoadChangeDbaseRecord>(r => r.IDENT8.Reset()),
                    NumberedRoadChangeDbaseRecord.Schema.IDENT8
                };

                yield return new object[]
                {
                    new Action<NumberedRoadChangeDbaseRecord>(r => r.RICHTING.Reset()),
                    NumberedRoadChangeDbaseRecord.Schema.RICHTING
                };

                yield return new object[]
                {
                    new Action<NumberedRoadChangeDbaseRecord>(r => r.VOLGNUMMER.Reset()),
                    NumberedRoadChangeDbaseRecord.Schema.VOLGNUMMER
                };
            }
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<NumberedRoadChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord>(records, 1, exception);

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)
                ),
                result,
                new FileProblemComparer());
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
        {
            var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
            record.WS_OIDN.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentIdOutOfRange(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidNumberedRoadNumberReturnsExpectedResult()
        {
            var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
            record.IDENT8.Value = "-1";
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NotNumberedRoadNumber("-1")),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidNumberedRoadDirectionReturnsExpectedResult()
        {
            var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
            record.RICHTING.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NumberedRoadDirectionMismatch(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidNumberedRoadOrdinalReturnsExpectedResult()
        {
            var record = _fixture.Create<NumberedRoadChangeDbaseRecord>();
            record.VOLGNUMMER.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).NumberedRoadOrdinalOutOfRange(-1)),
                result);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
