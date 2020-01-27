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

    public class RoadSegmentChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> _enumerator;

        public RoadSegmentChangeDbaseRecordsValidatorTests()
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
            _fixture.Customize<RoadSegmentChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue))},
                        METHODE = { Value = (short)_fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier },
                        BEHEERDER = { Value = _fixture.Create<OrganizationId>() },
                        MORFOLOGIE = { Value = (short)_fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                        STATUS = { Value = _fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                        WEGCAT = { Value = _fixture.Create<RoadSegmentCategory>().Translation.Identifier },
                        B_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                        E_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                        LSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue))},
                        RSTRNMID = { Value = new CrabStreetnameId(random.Next(1, int.MaxValue))},
                        TGBEP = { Value = (short)_fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier },
                        EVENTIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue))}
                    })
                    .OmitAutoProperties());

            _sut = new RoadSegmentChangeDbaseRecordsValidator();
            _enumerator = new List<RoadSegmentChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegsegment_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentChangeDbaseRecord>>(_sut);
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
                .CreateMany<RoadSegmentChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WS_OIDN.Value = index + 1;
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
                .CreateMany<RoadSegmentChangeDbaseRecord>(2)
                .Select((record, index) =>
                {
                    record.WS_OIDN.Value = index + 1;
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
        public void ValidateWithRecordsThatHaveTheSameRoadSegmentIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WS_OIDN.Value = 1;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierNotUnique(
                        new RoadSegmentId(1),
                        new RecordNumber(1))
                ),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsRoadSegmentIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WS_OIDN.Value = 0;
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
            Action<RoadSegmentChangeDbaseRecord> modifier, DbaseField field)
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
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
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.WS_OIDN.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.WS_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.RECORDTYPE.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.RECORDTYPE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.B_WK_OIDN.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.E_WK_OIDN.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.TGBEP.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.TGBEP
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.STATUS.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.STATUS
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.WEGCAT.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.WEGCAT
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.MORFOLOGIE.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.METHODE.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.METHODE
                };

                yield return new object[]
                {
                    new Action<RoadSegmentChangeDbaseRecord>(r => r.BEHEERDER.Value = null),
                    RoadSegmentChangeDbaseRecord.Schema.BEHEERDER
                };
            }
        }

        [Fact]
        public void ValidateWithProblematicRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentChangeDbaseRecord>(2)
                .ToArray();
            var exception = new Exception("problem");
            var enumerator = new ProblematicDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord>(records, 1, exception);

            var result = _sut.Validate(_entry, enumerator);

            Assert.Equal(
                ZipArchiveProblems.Single(
                    _entry.AtDbaseRecord(new RecordNumber(2)).HasDbaseRecordFormatError(exception)),
                result,
                new FileProblemComparer());
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidBeginRoadNodeIdReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.B_WK_OIDN.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).BeginRoadNodeIdOutOfRange(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidEndRoadNodeIdReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.E_WK_OIDN.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).EndRoadNodeIdOutOfRange(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidAccessRestrictionReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.TGBEP.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentAccessRestrictionMismatch(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidStatusReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.STATUS.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentStatusMismatch(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidGeometryDrawMethodReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.METHODE.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentGeometryDrawMethodMismatch(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidMorphologyReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.MORFOLOGIE.Value = -1;
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentMorphologyMismatch(-1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasInvalidCategoryReturnsExpectedResult()
        {
            var record = _fixture.Create<RoadSegmentChangeDbaseRecord>();
            record.WEGCAT.Value = "-1";
            var records = new [] { record }.ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RoadSegmentCategoryMismatch("-1")),
                result);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
