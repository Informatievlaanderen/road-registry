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

    public class RoadSegmentWidthChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentWidthChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadSegmentWidthChangeDbaseRecord> _enumerator;

        public RoadSegmentWidthChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordType();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentWidth();
            _fixture.CustomizeRoadSegmentPosition();
            _fixture.Customize<RoadSegmentWidthChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentWidthChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        WB_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        VANPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOTPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        BREEDTE = { Value = (short)_fixture.Create<RoadSegmentWidth>().ToInt32() }
                    })
                    .OmitAutoProperties());

            _sut = new RoadSegmentWidthChangeDbaseRecordsValidator();
            _enumerator = new List<RoadSegmentWidthChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attwegbreedte_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentWidthChangeDbaseRecord>>(_sut);
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
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WB_OIDN.Value = index + 1;
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
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.WB_OIDN.Value = index + 1;
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
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.WB_OIDN.Value = 1;
                    if (index == 0)
                    {
                        record.RECORDTYPE.Value = (short) RecordType.Identical.Translation.Identifier;
                    }
                    else if(index == 1)
                    {
                        record.RECORDTYPE.Value = (short) RecordType.Removed.Translation.Identifier;
                    }
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
        public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierAndHaveAddedAndRemovedAsRecordTypeReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.WB_OIDN.Value = 1;
                    if (index == 0)
                    {
                        record.RECORDTYPE.Value = (short) RecordType.Added.Translation.Identifier;
                    }
                    else if(index == 1)
                    {
                        record.RECORDTYPE.Value = (short) RecordType.Removed.Translation.Identifier;
                    }

                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
        }


        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WB_OIDN.Value = 0;
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
            Action<RoadSegmentWidthChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<RoadSegmentWidthChangeDbaseRecord>();
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
                    new Action<RoadSegmentWidthChangeDbaseRecord>(r => r.WS_OIDN.Reset()),
                    RoadSegmentWidthChangeDbaseRecord.Schema.WS_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentWidthChangeDbaseRecord>(r => r.RECORDTYPE.Reset()),
                    RoadSegmentWidthChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentWidthChangeDbaseRecord>(r => r.WB_OIDN.Reset()),
                    RoadSegmentWidthChangeDbaseRecord.Schema.WB_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentWidthChangeDbaseRecord>(r => r.BREEDTE.Reset()),
                    RoadSegmentWidthChangeDbaseRecord.Schema.BREEDTE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentWidthChangeDbaseRecord>(r => r.VANPOSITIE.Reset()),
                    RoadSegmentWidthChangeDbaseRecord.Schema.VANPOSITIE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentWidthChangeDbaseRecord>(r => r.TOTPOSITIE.Reset()),
                    RoadSegmentWidthChangeDbaseRecord.Schema.TOTPOSITIE
                };
            }
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentWidthChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentWidthChangeDbaseRecord>(records, 1, exception);

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
                result,
                new FileProblemComparer());
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidWidthReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentWidthChangeDbaseRecord>();
            record.BREEDTE.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).WidthOutOfRange(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidRoadSegmentIdReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentWidthChangeDbaseRecord>();
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
