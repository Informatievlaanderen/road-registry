namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class GradeSeparatedJunctionChangeDbaseRecordsTranslatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<GradeSeparatedJunctionChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly GradeSeparatedJunctionChangeDbaseRecordsTranslator _sut;

    public GradeSeparatedJunctionChangeDbaseRecordsTranslatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeGradeSeparatedJunctionId();
        _fixture.CustomizeGradeSeparatedJunctionType();
        _fixture.Customize<GradeSeparatedJunctionChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new GradeSeparatedJunctionChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    OK_OIDN = { Value = new GradeSeparatedJunctionId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)_fixture.Create<GradeSeparatedJunctionType>().Translation.Identifier },
                    BO_WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    ON_WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() }
                })
                .OmitAutoProperties());

        _sut = new GradeSeparatedJunctionChangeDbaseRecordsTranslator();
        _enumerator = new List<GradeSeparatedJunctionChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("rltogkruising_all.dbf");
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsTranslator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<GradeSeparatedJunctionChangeDbaseRecord>>(_sut);
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
            .CreateMany<GradeSeparatedJunctionChangeDbaseRecord>(new Random().Next(1, 4))
            .Select((record, index) =>
            {
                record.OK_OIDN.Value = index + 1;
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
                            new AddGradeSeparatedJunction(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new GradeSeparatedJunctionId(current.OK_OIDN.Value),
                                GradeSeparatedJunctionType.ByIdentifier[current.TYPE.Value],
                                new RoadSegmentId(current.BO_WS_OIDN.Value),
                                new RoadSegmentId(current.ON_WS_OIDN.Value)
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        nextChanges = previousChanges.AppendChange(
                            new RemoveGradeSeparatedJunction(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new GradeSeparatedJunctionId(current.OK_OIDN.Value)
                            )
                        );
                        break;
                }

                return nextChanges;
            });
        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }
}