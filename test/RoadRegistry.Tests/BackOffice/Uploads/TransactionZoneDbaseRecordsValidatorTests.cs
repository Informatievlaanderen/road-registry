namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using Schema;
    using Xunit;

    public class TransactionZoneDbaseRecordsValidatorTests : IDisposable
    {
        private readonly TransactionZoneDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<TransactionZoneDbaseRecord> _enumerator;

        public TransactionZoneDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeReason();
            _fixture.CustomizeOperatorName();
            _fixture.CustomizeOrganizationId();
            _fixture.Customize<TransactionZoneDbaseRecord>(
                composer => composer
                    .FromFactory(random => new TransactionZoneDbaseRecord
                    {
                        SOURCEID = {Value = random.Next(1, 5)},
                        TYPE = {Value = random.Next(1, 9999)},
                        BESCHRIJV = { Value = _fixture.Create<Reason>().ToString() },
                        OPERATOR = { Value = _fixture.Create<OperatorName>().ToString() },
                        ORG = { Value = _fixture.Create<OrganizationId>().ToString() },
                        APPLICATIE =
                        {
                            Value = new string(_fixture
                                .CreateMany<char>(TransactionZoneDbaseRecord.Schema.APPLICATIE.Length.ToInt32())
                                .ToArray())
                        }
                    })
                    .OmitAutoProperties());

            _sut = new TransactionZoneDbaseRecordsValidator();
            _enumerator = new List<TransactionZoneDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("transactiezone.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<TransactionZoneDbaseRecord>>(_sut);
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
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.None,
                result);
        }

        [Fact]
        public void ValidateWithMoreThanOneRecordReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(2)
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.HasTooManyDbaseRecords(1, 2)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingReasonReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.BESCHRIJV.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.BESCHRIJV)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingOperatorNameReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.OPERATOR.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.OPERATOR)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatIsMissingOrganizationIdReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.ORG.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).RequiredFieldIsNull(TransactionZoneDbaseRecord.Schema.ORG)),
                result);
        }

        [Fact]
        public void ValidateWithRecordThatHasOrganizationIdOutOfRangeReturnsExpectedResult()
        {
            var value = string.Empty;
            var records = _fixture
                .CreateMany<TransactionZoneDbaseRecord>(1)
                .Select(record =>
                {
                    record.ORG.Value = value;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Single(_entry.AtDbaseRecord(new RecordNumber(1)).OrganizationIdOutOfRange(value)),
                result);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
