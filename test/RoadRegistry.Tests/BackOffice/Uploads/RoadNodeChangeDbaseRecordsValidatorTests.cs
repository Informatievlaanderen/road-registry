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

    public class RoadNodeChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadNodeChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> _enumerator;
        private readonly ZipArchiveValidationContext _context;

        public RoadNodeChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadNodeType();
            _fixture.Customize<RoadNodeChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadNodeChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        WEGKNOOPID = { Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                        TYPE = { Value = (short)_fixture.Create<RoadNodeType>().Translation.Identifier }
                    })
                    .OmitAutoProperties());

            _sut = new RoadNodeChangeDbaseRecordsValidator();
            _enumerator = new List<RoadNodeChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegknoop_all.dbf");
            _context = ZipArchiveValidationContext.Empty;
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadNodeChangeDbaseRecord>>(_sut);
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, _enumerator, _context));
        }

        [Fact]
        public void ValidateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null, _context));
        }

        [Fact]
        public void ValidateContextCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, _enumerator, null));
        }

        [Fact]
        public void ValidateWithoutRecordsReturnsExpectedResult()
        {
            var (result, context) = _sut.Validate(_entry, _enumerator, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.HasNoDbaseRecords(false)),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadNodeChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WEGKNOOPID.Value = index + 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheirRecordTypeMismatchReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadNodeChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.WEGKNOOPID.Value = index + 1;
                    record.RECORDTYPE.Value = -1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

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
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheSameRoadNodeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadNodeChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WEGKNOOPID.Value = 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
                        new RoadNodeId(1),
                        new RecordNumber(1))
                ),
                result);
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsRoadNodeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadNodeChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WEGKNOOPID.Value = 0;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Many(
                    _entry.AtDbaseRecord(new RecordNumber(1)).IdentifierZero(),
                    _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierZero()
                ),
                result);
            Assert.Same(_context, context);
        }

        [Theory]
        [MemberData(nameof(ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases))]
        public void ValidateWithRecordsThatHaveNullAsRequiredFieldValueReturnsExpectedResult(
            Action<RoadNodeChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<RoadNodeChangeDbaseRecord>();
            modifier(record);
            var records = new[] {record}.ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Contains(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(field), result);
            Assert.Same(_context, context);
        }

        public static IEnumerable<object[]> ValidateWithRecordsThatHaveNullAsRequiredFieldValueCases
        {
            get
            {
                yield return new object[]
                {
                    new Action<RoadNodeChangeDbaseRecord>(r => r.WEGKNOOPID.Reset()),
                    RoadNodeChangeDbaseRecord.Schema.WEGKNOOPID
                };

                yield return new object[]
                {
                    new Action<RoadNodeChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                    RoadNodeChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<RoadNodeChangeDbaseRecord>(r => r.TYPE.Reset()),
                    RoadNodeChangeDbaseRecord.Schema.TYPE
                };
            }
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadNodeChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<RoadNodeChangeDbaseRecord>(records, 1, exception);

            var (result, context) = _sut.Validate(_entry, enumerator, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
                result,
                new FileProblemComparer());
            Assert.Same(_context, context);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidRoadNodeTypeReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadNodeChangeDbaseRecord>();
            record.TYPE.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var (result, context) = _sut.Validate(_entry, records, _context);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadNodeTypeMismatch(-1)),
                result);
            Assert.Same(_context, context);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
