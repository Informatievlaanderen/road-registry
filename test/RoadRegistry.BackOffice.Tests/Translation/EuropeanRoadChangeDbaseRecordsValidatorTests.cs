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
        private IEnumerator<EuropeanRoadChangeDbaseRecord> _enumerator;

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
            _enumerator = new List<EuropeanRoadChangeDbaseRecord>().GetEnumerator();
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
                ZipArchiveErrors.None.NoDbaseRecords(_entry.Name),
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
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None,
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
                .GetEnumerator();

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
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.EU_OIDN.Value = 0;
                    return record;
                })
                .GetEnumerator();

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
                .CreateMany<EuropeanRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.EU_OIDN.Value = null;
                    return record;
                })
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierMissing(_entry.Name, new RecordNumber(1))
                    .IdentifierMissing(_entry.Name, new RecordNumber(2)),
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
                ZipArchiveErrors
                    .None
                    .DbaseRecordFormatError(_entry.Name, new RecordNumber(2), exception),
                result,
                new ErrorComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
