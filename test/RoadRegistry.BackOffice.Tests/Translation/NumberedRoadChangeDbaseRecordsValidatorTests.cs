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

        [Fact]
        public void ValidateWithRecordsThatAreMissingAnAttributeIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<NumberedRoadChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.GW_OIDN.Value = null;
                    return record;
                })
                .ToDbaseRecordEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveProblems.Many(
                    _entry.AtDbaseRecord(new RecordNumber(1)).IdentifierMissing(),
                    _entry.AtDbaseRecord(new RecordNumber(2)).IdentifierMissing()
                ),
                result);
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

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
