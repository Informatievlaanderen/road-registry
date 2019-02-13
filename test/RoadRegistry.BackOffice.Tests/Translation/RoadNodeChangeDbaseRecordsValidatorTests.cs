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

    public class RoadNodeChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadNodeChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;

        public RoadNodeChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadNodeType();
            _fixture.Customize<RoadNodeChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadNodeChangeDbaseRecord
                    {
                        RecordType = {Value = random.Next(1, 5)},
                        TransactID = {Value = random.Next(1, 9999)},
                        WEGKNOOPID = { Value = new RoadNodeId(random.Next(1, int.MaxValue))},
                        TYPE = { Value = (short)_fixture.Create<RoadNodeType>().Translation.Identifier }
                    })
                    .OmitAutoProperties());

            _sut = new RoadNodeChangeDbaseRecordsValidator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegknoop_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadNodeChangeDbaseRecord>>(_sut);
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, new RoadNodeChangeDbaseRecord[0]));
        }

        [Fact]
        public void ValidateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null));
        }

        [Fact]
        public void ValidateWithoutRecordsReturnsExpectedResult()
        {
            var result = _sut.Validate(_entry, new RoadNodeChangeDbaseRecord[0]);

            Assert.Equal(
                ZipArchiveErrors.None.NoDbaseRecords(_entry.Name),
                result);
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
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None,
                result);
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
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierNotUnique(
                        _entry.Name,
                        new RoadNodeId(1),
                        new RecordNumber(2),
                        new RecordNumber(1)),
                result);
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
        public void ValidateWithRecordsThatAreMissingAnRoadNodeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadNodeChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WEGKNOOPID.Value = null;
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
