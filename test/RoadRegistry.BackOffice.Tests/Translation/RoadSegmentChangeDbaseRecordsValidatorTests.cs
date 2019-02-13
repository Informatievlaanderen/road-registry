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

    public class RoadSegmentChangeDbaseRecordsValidatorTests : IDisposable
    {
        private readonly RoadSegmentChangeDbaseRecordsValidator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private IEnumerator<RoadSegmentChangeDbaseRecord> _enumerator;

        public RoadSegmentChangeDbaseRecordsValidatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadSegmentGeometryDrawMethod();
            _fixture.CustomizeMaintenanceAuthorityId();
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
                        BEHEERDER = { Value = _fixture.Create<MaintenanceAuthorityId>() },
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
            _enumerator = new List<RoadSegmentChangeDbaseRecord>().GetEnumerator();
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
                ZipArchiveErrors.None.NoDbaseRecords(_entry.Name),
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
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors.None,
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
                .GetEnumerator();

            var result = _sut.Validate(_entry, records);

            Assert.Equal(
                ZipArchiveErrors
                    .None
                    .IdentifierNotUnique(
                        _entry.Name,
                        new RoadSegmentId(1),
                        new RecordNumber(2),
                        new RecordNumber(1)),
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
        public void ValidateWithRecordsThatAreMissingAnRoadSegmentIdentifierReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentChangeDbaseRecord>(2)
                .Select(record =>
                {
                    record.WS_OIDN.Value = null;
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

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
