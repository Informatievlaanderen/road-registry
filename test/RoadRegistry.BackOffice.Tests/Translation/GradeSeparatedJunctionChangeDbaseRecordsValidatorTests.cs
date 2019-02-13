namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Model;
    using Xunit;

    public class GradeSeparatedJunctionChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly GradeSeparatedJunctionChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;

        public GradeSeparatedJunctionChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeGradeSeparatedJunctionId();
            _fixture.CustomizeGradeSeparatedJunctionType();
            _fixture.Customize<GradeSeparatedJunctionChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new GradeSeparatedJunctionChangeDbaseRecord
                    {
                        RecordType = {Value = random.Next(1, 5)},
                        TransactID = {Value = random.Next(1, 9999)},
                        OK_OIDN = {Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue))},
                        TYPE = { Value = (short)_fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                        BO_WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        ON_WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                    })
                    .OmitAutoProperties());

            _sut = new GradeSeparatedJunctionChangeDbaseRecordsValidator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("rltogkruising_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<GradeSeparatedJunctionChangeDbaseRecord>>(_sut);
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, new GradeSeparatedJunctionChangeDbaseRecord[0]));
        }

        [Fact]
        public void ValidateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null));
        }

        [Fact]
        public void ValidateWithoutRecordsReturnsExpectedResult()
        {
            var result = _sut.Validate(_entry, new GradeSeparatedJunctionChangeDbaseRecord[0]);

            Assert.Equal(
                ZipArchiveErrors.None.NoDbaseRecords(_entry.Name),
                result);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.OK_OIDN.Value = index + 1;
                    return record;
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None,
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheSameGradeSeparatedJunctionIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.OK_OIDN.Value = 1;
                    return record;
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierNotUnique(
                        _entry.Name,
                        new GradeSeparatedJunctionId(1),
                        new RecordNumber(2),
                        new RecordNumber(1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsGradeSeparatedJunctionIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.OK_OIDN.Value = 0;
                    return record;
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierZero(_entry.Name, new RecordNumber(1))
                    .IdentifierZero(_entry.Name, new RecordNumber(2)),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatAreMissingAnGradeSeparatedJunctionIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.OK_OIDN.Value = null;
                    return record;
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierMissing(_entry.Name, new RecordNumber(1))
                    .IdentifierMissing(_entry.Name, new RecordNumber(2)),
                result);
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
