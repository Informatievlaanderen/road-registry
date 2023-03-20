namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;
using Xunit;

public class NationalRoadChangeDbaseRecordsTranslatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<NationalRoadChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly NationalRoadChangeDbaseRecordsTranslator _sut;

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
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    NW_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    IDENT2 = { Value = _fixture.Create<NationalRoadNumber>().ToString() }
                })
                .OmitAutoProperties());

        _sut = new NationalRoadChangeDbaseRecordsTranslator();
        _enumerator = new List<NationalRoadChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("attnationweg_all.dbf");
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsTranslator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<NationalRoadChangeDbaseRecord>>(_sut);
    }

    [Fact]
    public void TranslateChangesCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Translate(_entry, _enumerator, null));
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
            .CreateMany<NationalRoadChangeDbaseRecord>(new Random().Next(1, 4))
            .Select((record, index) =>
            {
                record.NW_OIDN.Value = index + 1;
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
                        nextChanges = previousChanges.AppendChange(
                            new AddRoadSegmentToNationalRoad(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new AttributeId(current.NW_OIDN.Value),
                                new RoadSegmentId(current.WS_OIDN.Value),
                                NationalRoadNumber.Parse(current.IDENT2.Value)));
                        break;
                    case RecordType.RemovedIdentifier:
                        nextChanges = previousChanges.AppendChange(
                            new RemoveRoadSegmentFromNationalRoad(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new AttributeId(current.NW_OIDN.Value),
                                new RoadSegmentId(current.WS_OIDN.Value),
                                NationalRoadNumber.Parse(current.IDENT2.Value)));
                        break;
                }

                return nextChanges;
            });
        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }
}
