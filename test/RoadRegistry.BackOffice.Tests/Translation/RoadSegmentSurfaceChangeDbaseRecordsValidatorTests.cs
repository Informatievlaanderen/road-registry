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

    public class RoadSegmentSurfaceChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentSurfaceChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;

        public RoadSegmentSurfaceChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentSurfaceType();
            _fixture.CustomizeRoadSegmentPosition();
            _fixture.Customize<RoadSegmentSurfaceChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new RoadSegmentSurfaceChangeDbaseRecord
                    {
                        RecordType = {Value = random.Next(1, 5)},
                        TransactID = {Value = random.Next(1, 9999)},
                        WV_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        VAN_POSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TOT_POSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                        TYPE = { Value = (short)_fixture.Create<RoadSegmentSurfaceType>().Translation.Identifier }
                    })
                    .OmitAutoProperties());

            _sut = new RoadSegmentSurfaceChangeDbaseRecordsValidator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attwegverharding_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsValidator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsValidator<RoadSegmentSurfaceChangeDbaseRecord>>(_sut);
        }

        [Fact]
        public void ValidateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(null, new RoadSegmentSurfaceChangeDbaseRecord[0]));
        }

        [Fact]
        public void ValidateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Validate(_entry, null));
        }

        [Fact]
        public void ValidateWithoutRecordsReturnsExpectedResult()
        {
            var result = _sut.Validate(_entry, new RoadSegmentSurfaceChangeDbaseRecord[0]);

            Assert.Equal(
                ZipArchiveErrors.None.NoDbaseRecords(_entry.Name),
                result);
        }

        [Fact]
        public void ValidateWithValidRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WV_OIDN.Value = index + 1;
                    return record;
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None,
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveTheSameAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WV_OIDN.Value = 1;
                    return record;
                });

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierNotUnique(
                        _entry.Name,
                        new AttributeId(1),
                        new RecordNumber(2),
                        new RecordNumber(1)),
                result);
        }

        [Fact]
        public void ValidateWithRecordsThatHaveZeroAsAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WV_OIDN.Value = 0;
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
        public void ValidateWithRecordsThatAreMissingAnAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentSurfaceChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WV_OIDN.Value = null;
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
