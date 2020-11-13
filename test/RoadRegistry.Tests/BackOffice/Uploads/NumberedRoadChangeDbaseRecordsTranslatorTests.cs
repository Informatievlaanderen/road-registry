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

    public class NumberedRoadChangeDbaseRecordsTranslatorTests : IDisposable
    {
        private readonly NumberedRoadChangeDbaseRecordsTranslator _sut;
        private readonly ZipArchive _archive;
        private readonly MemoryStream _stream;
        private readonly ZipArchiveEntry _entry;
        private readonly Fixture _fixture;
        private readonly IDbaseRecordEnumerator<NumberedRoadChangeDbaseRecord> _enumerator;

        public NumberedRoadChangeDbaseRecordsTranslatorTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRecordType();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeNumberedRoadNumber();
            _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            _fixture.CustomizeRoadSegmentNumberedRoadDirection();
            _fixture.Customize<NumberedRoadChangeDbaseRecord>(
                composer => composer
                    .FromFactory(random => new NumberedRoadChangeDbaseRecord
                    {
                        RECORDTYPE = {Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                        TRANSACTID = {Value = (short)random.Next(1, 9999)},
                        GW_OIDN = {Value = new AttributeId(random.Next(1, int.MaxValue))},
                        WS_OIDN = {Value = _fixture.Create<RoadSegmentId>().ToInt32()},
                        IDENT8 = {Value = _fixture.Create<NumberedRoadNumber>().ToString()},
                        RICHTING = {Value = (short) _fixture.Create<RoadSegmentNumberedRoadDirection>().Translation.Identifier},
                        VOLGNUMMER = {Value = _fixture.Create<RoadSegmentNumberedRoadOrdinal>().ToInt32()}
                    })
                    .OmitAutoProperties());

            _sut = new NumberedRoadChangeDbaseRecordsTranslator();
            _enumerator = new List<NumberedRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
            _stream = new MemoryStream();
            _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
            _entry = _archive.CreateEntry("attgenumweg_all.dbf");
        }

        [Fact]
        public void IsZipArchiveDbaseRecordsTranslator()
        {
            Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<NumberedRoadChangeDbaseRecord>>(_sut);
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
                .CreateMany<NumberedRoadChangeDbaseRecord>(new Random().Next(1, 5))
                .Select((record, index) =>
                {
                    record.GW_OIDN.Value = index + 1;
                    switch (index % 2)
                    {
                        case 0:
                            record.RECORDTYPE.Value = (short)RecordType.Added.Translation.Identifier;
                            break;
                        case 1:
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
                                new Uploads.AddRoadSegmentToNumberedRoad(
                                    new RecordNumber(Array.IndexOf(records, current) + 1),
                                    new AttributeId(current.GW_OIDN.Value),
                                    new RoadSegmentId(current.WS_OIDN.Value),
                                    NumberedRoadNumber.Parse(current.IDENT8.Value),
                                    RoadSegmentNumberedRoadDirection.ByIdentifier[current.RICHTING.Value],
                                    new RoadSegmentNumberedRoadOrdinal(current.VOLGNUMMER.Value)));
                            break;
                        case RecordType.RemovedIdentifier:
                            nextChanges = previousChanges.Append(
                                new Uploads.RemoveRoadSegmentFromNumberedRoad(
                                    new RecordNumber(Array.IndexOf(records, current) + 1),
                                    new AttributeId(current.GW_OIDN.Value),
                                    new RoadSegmentId(current.WS_OIDN.Value),
                                    NumberedRoadNumber.Parse(current.IDENT8.Value)));
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
