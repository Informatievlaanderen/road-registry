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

    public class NationalRoadChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly NationalRoadChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> _enumerator;

        public NationalRoadChangeDbaseRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordType();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeNationalRoadNumber();
            _fixture.Customize<NationalRoadChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new NationalRoadChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)_fixture.Create<RecordType>().Translation.Identifier},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        NW_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        IDENT2 = {Value = _fixture.Create<NationalRoadNumber>().ToString()}
                    })
                    .OmitAutoProperties());

            _sut = new NationalRoadChangeDbaseRecordsTranslator();
            _enumerator = new List<NationalRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attnationweg_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<NationalRoadChangeDbaseRecord>>(_sut);
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
                .CreateMany<NationalRoadChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.NW_OIDN.Value = index + 1;
                    record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();

            var result = _sut.Translate(_entry, enumerator, TranslatedChanges.Empty);

            var expected = records.Aggregate(
                TranslatedChanges.Empty,
                (changes, current) => changes.Append(
                    new Uploads.AddRoadSegmentToNationalRoad(
                        new RecordNumber(Array.IndexOf(records, current) + 1),
                        new AttributeId(current.NW_OIDN.Value),
                        new RoadSegmentId(current.WS_OIDN.Value),
                        NationalRoadNumber.Parse(current.IDENT2.Value)))
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
