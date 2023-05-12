namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class RoadNodeChangeDbaseRecordsTranslatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadNodeChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadNodeChangeDbaseRecordsTranslator _sut;

    public RoadNodeChangeDbaseRecordsTranslatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeType();
        _fixture.Customize<RoadNodeChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadNodeChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)_fixture.Create<RecordType>().Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WEGKNOOPID = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    TYPE = { Value = (short)_fixture.Create<RoadNodeType>().Translation.Identifier }
                })
                .OmitAutoProperties());

        _sut = new RoadNodeChangeDbaseRecordsTranslator();
        _enumerator = new List<RoadNodeChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("wegknoop_all.dbf");
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsTranslator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<RoadNodeChangeDbaseRecord>>(_sut);
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
            .CreateMany<RoadNodeChangeDbaseRecord>(new Random().Next(1, 4))
            .Select((record, index) =>
            {
                record.WEGKNOOPID.Value = index + 1;
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
                        nextChanges = previousChanges.AppendChange(
                            new AddRoadNode(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new RoadNodeId(current.WEGKNOOPID.Value),
                                RoadNodeType.ByIdentifier[current.TYPE.Value]
                            )
                        );
                        break;
                    case RecordType.ModifiedIdentifier:
                        nextChanges = previousChanges.AppendChange(
                            new ModifyRoadNode(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new RoadNodeId(current.WEGKNOOPID.Value),
                                RoadNodeType.ByIdentifier[current.TYPE.Value]
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        nextChanges = previousChanges.AppendChange(
                            new RemoveRoadNode(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new RoadNodeId(current.WEGKNOOPID.Value)
                            )
                        );
                        break;
                }

                return nextChanges;
            });

        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }
}