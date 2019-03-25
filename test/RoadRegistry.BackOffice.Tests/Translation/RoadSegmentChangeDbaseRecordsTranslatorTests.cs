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

    public class RoadSegmentChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly RoadSegmentChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> _enumerator;

        public RoadSegmentChangeDbaseRecordsTranslatorTests()
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

            _sut = new RoadSegmentChangeDbaseRecordsTranslator();
            _enumerator = new List<RoadSegmentChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("wegsegment_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<RoadSegmentChangeDbaseRecord>>(_sut);
        }

        [Fact]
        public void TranslateEntryCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Translate(null, _enumerator, TranslatedChanges.Empty));
        }

        [Fact]
        public void TranslateRecordsCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, null, TranslatedChanges.Empty));
        }

        [Fact]
        public void TranslateChangesCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, _enumerator, null));
        }

        [Fact]
        public void TranslateWithoutRecordsReturnsExpectedResult()
        {
            var result = _sut.Translate(_entry, _enumerator, TranslatedChanges.Empty);

            Assert.Equal(
                TranslatedChanges.Empty,
                result);
        }

        [Fact]
        public void TranslateWithRecordsReturnsExpectedResult()
        {
            var records = _fixture
                .CreateMany<RoadSegmentChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.WS_OIDN.Value = index + 1;
                    record.RECORDTYPE.Value = RecordTypes.Added;
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();

            var result = _sut.Translate(_entry, enumerator, TranslatedChanges.Empty);


            var expected = records.Aggregate(
                TranslatedChanges.Empty,
                (changes, current) => changes.Append(
                    new AddRoadSegment(
                        new RecordNumber(Array.IndexOf(records, current) + 1),
                        new RoadSegmentId(current.WS_OIDN.Value.GetValueOrDefault()),
                        new RoadNodeId(current.B_WK_OIDN.Value.GetValueOrDefault()),
                        new RoadNodeId(current.E_WK_OIDN.Value.GetValueOrDefault()),
                        new MaintenanceAuthorityId(current.BEHEERDER.Value),
                        RoadSegmentGeometryDrawMethod.ByIdentifier[current.METHODE.Value.GetValueOrDefault()],
                        RoadSegmentMorphology.ByIdentifier[current.MORFOLOGIE.Value.GetValueOrDefault()],
                        RoadSegmentStatus.ByIdentifier[current.STATUS.Value.GetValueOrDefault()],
                        RoadSegmentCategory.ByIdentifier[current.WEGCAT.Value],
                        RoadSegmentAccessRestriction.ByIdentifier[current.TGBEP.Value.GetValueOrDefault()],
                        current.LSTRNMID.Value.HasValue ? new CrabStreetnameId(current.LSTRNMID.Value.GetValueOrDefault()) : default,
                        current.RSTRNMID.Value.HasValue ? new CrabStreetnameId(current.RSTRNMID.Value.GetValueOrDefault()) : default
                    )
                )
            );
            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}