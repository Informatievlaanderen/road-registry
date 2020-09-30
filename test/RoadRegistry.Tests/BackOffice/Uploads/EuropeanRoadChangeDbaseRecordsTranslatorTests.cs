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

    public class EuropeanRoadChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly EuropeanRoadChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<EuropeanRoadChangeDbaseRecord> _enumerator;

        public EuropeanRoadChangeDbaseRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordType();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeEuropeanRoadNumber();
            _fixture.Customize<EuropeanRoadChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new EuropeanRoadChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)_fixture.Create<RecordType>().Translation.Identifier},
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        EU_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        EUNUMMER = {Value = _fixture.Create<EuropeanRoadNumber>().ToString()}
                    })
                    .OmitAutoProperties());

            _sut = new EuropeanRoadChangeDbaseRecordsTranslator();
            _enumerator = new List<EuropeanRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("atteuropweg_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<EuropeanRoadChangeDbaseRecord>>(_sut);
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
                .CreateMany<EuropeanRoadChangeDbaseRecord>(new Random().Next(1, 4))
                .Select((record, index) =>
                {
                    record.EU_OIDN.Value = index + 1;
                    switch (index % 3)
                    {
                        case 0:
                            record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                            break;
                        case 1:
                            record.RECORDTYPE.Value = (short)RecordType.Modified.Translation.Identifier;
                            break;
                        case 2:
                            record.RECORDTYPE.Value = (short)RecordType.Removed.Translation.Identifier;
                            break;
                    }
                    return record;
                })
                .ToArray();
            var enumerator = records.ToDbaseRecordEnumerator();

            var result = _sut.Translate(_entry, enumerator, TranslatedChanges.Empty);

            var expected = records.Aggregate(
                TranslatedChanges.Empty,
                (previousChanges, current) =>
                {
                    var nextChanges = previousChanges;
                    switch (current.RECORDTYPE.Value)
                    {
                        case RecordType.AddedIdentifier:
                            nextChanges = previousChanges.Append(
                                new Uploads.AddRoadSegmentToEuropeanRoad(
                                    new RecordNumber(Array.IndexOf(records, current) + 1),
                                    new AttributeId(current.EU_OIDN.Value),
                                    new RoadSegmentId(current.WS_OIDN.Value),
                                    EuropeanRoadNumber.Parse(current.EUNUMMER.Value)));
                            break;
                        case RecordType.ModifiedIdentifier:
                            break; // modify case is not handled - we need to verify that this does not appear
                        case RecordType.RemovedIdentifier:
                            nextChanges = previousChanges.Append(
                                new Uploads.RemoveRoadSegmentFromEuropeanRoad(
                                    new RecordNumber(Array.IndexOf(records, current) + 1),
                                    new AttributeId(current.EU_OIDN.Value),
                                    new RoadSegmentId(current.WS_OIDN.Value),
                                    EuropeanRoadNumber.Parse(current.EUNUMMER.Value)));
                            break;
                    }

                    return nextChanges;
                });
            Assert.Equal(expected,result, new TranslatedChangeEqualityComparer());
        }

        public void Dispose()
        {
            _archive?.Dispose();
            _stream?.Dispose();
        }
    }
}
