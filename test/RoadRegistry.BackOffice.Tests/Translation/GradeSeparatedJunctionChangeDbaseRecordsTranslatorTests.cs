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

    public class GradeSeparatedJunctionChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly GradeSeparatedJunctionChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> _enumerator;

        public GradeSeparatedJunctionChangeDbaseRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeGradeSeparatedJunctionId();
            _fixture.CustomizeGradeSeparatedJunctionType();
            _fixture.Customize<GradeSeparatedJunctionChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new GradeSeparatedJunctionChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)random.Next(1, 5)},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        OK_OIDN = {Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue))},
                        TYPE = { Value = (short)_fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                        BO_WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        ON_WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                    })
                    .OmitAutoProperties());

            _sut = new GradeSeparatedJunctionChangeDbaseRecordsTranslator();
            _enumerator = new List<GradeSeparatedJunctionChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("rltogkruising_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<GradeSeparatedJunctionChangeDbaseRecord>>(_sut);
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
                .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.OK_OIDN.Value = index + 1;
                    record.RECORDTYPE.Value = RecordTypes.Added;
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();

            var result = _sut.Translate(_entry, enumerator, TranslatedChanges.Empty);

            var expected = records.Aggregate(
                TranslatedChanges.Empty,
                (changes, current) => changes.Append(
                    new AddGradeSeparatedJunction(
                        new GradeSeparatedJunctionId(current.OK_OIDN.Value.GetValueOrDefault()),
                        GradeSeparatedJunctionType.ByIdentifier[current.TYPE.Value.GetValueOrDefault()],
                        new RoadSegmentId(current.BO_WS_OIDN.Value.GetValueOrDefault()),
                        new RoadSegmentId(current.ON_WS_OIDN.Value.GetValueOrDefault())
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