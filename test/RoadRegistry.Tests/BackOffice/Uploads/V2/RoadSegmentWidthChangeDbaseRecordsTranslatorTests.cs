namespace RoadRegistry.Tests.BackOffice.Uploads.V2;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Validation;
using Xunit;

public class RoadSegmentWidthChangeDbaseRecordsTranslatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadSegmentWidthChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadSegmentWidthChangeDbaseRecordsTranslator _sut;

    public RoadSegmentWidthChangeDbaseRecordsTranslatorTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRecordType();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentPosition();
        _fixture.Customize<RoadSegmentWidthChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentWidthChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)new Generator<RecordType>(_fixture).First(candidate => candidate.IsAnyOf(RecordType.Added, RecordType.Identical, RecordType.Removed)).Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WB_OIDN = { Value = new AttributeId(random.Next(1, int.MaxValue)) },
                    WS_OIDN = { Value = _fixture.Create<RoadSegmentId>().ToInt32() },
                    VANPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                    TOTPOSITIE = { Value = _fixture.Create<RoadSegmentPosition>().ToDouble() },
                    BREEDTE = { Value = (short)_fixture.Create<RoadSegmentWidth>().ToInt32() }
                })
                .OmitAutoProperties());

        _sut = new RoadSegmentWidthChangeDbaseRecordsTranslator();
        _enumerator = new List<RoadSegmentWidthChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("attwegbreedte_all.dbf");
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsTranslator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<RoadSegmentWidthChangeDbaseRecord>>(_sut);
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
    public void TranslateWithChangedRecordsForModifyRoadSegmentReturnsExpectedResult()
    {
        var segment = _fixture.Create<ModifyRoadSegment>();
        var records = _fixture
            .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WB_OIDN.Value = index + 1;
                record.WS_OIDN.Value = segment.Id;
                record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                if (index == 0) // force at least one lane change to promote the provisional change to an actual change
                    record.RECORDTYPE.Value = RecordType.AddedIdentifier;

                return record;
            })
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();
        var changes = TranslatedChanges.Empty.AppendProvisionalChange(segment);

        var result = _sut.Translate(_entry, enumerator, changes);

        var expected =
            TranslatedChanges.Empty.AppendChange(
                records
                    .Where(record => record.RECORDTYPE.Value != (short)RecordType.Removed.Translation.Identifier)
                    .Aggregate(
                        segment,
                        (current, record) => current.WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(record.WB_OIDN.Value),
                                new RoadSegmentWidth(record.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value)))
                        )
                    )
            );

        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }

    [Fact]
    public void TranslateWithIdenticalRecordsForModifyRoadSegmentReturnsExpectedResult()
    {
        var segment = _fixture.Create<ModifyRoadSegment>();
        var records = _fixture
            .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WB_OIDN.Value = index + 1;
                record.WS_OIDN.Value = segment.Id;
                record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                record.RECORDTYPE.Value = RecordType.IdenticalIdentifier;
                return record;
            })
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();
        var changes = TranslatedChanges.Empty.AppendProvisionalChange(segment);

        var result = _sut.Translate(_entry, enumerator, changes);

        var expected = records
            .Where(record => record.RECORDTYPE.Value != (short)RecordType.Removed.Translation.Identifier)
            .Aggregate(
                segment,
                (current, record) => current.WithWidth(
                    new RoadSegmentWidthAttribute(
                        new AttributeId(record.WB_OIDN.Value),
                        new RoadSegmentWidth(record.BREEDTE.Value),
                        new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value)),
                        new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value)))
                )
            );
        var expectedResult = TranslatedChanges.Empty.AppendProvisionalChange(expected);

        Assert.Equal(expectedResult, result, new TranslatedChangeEqualityComparer());

        Assert.True(result.TryFindRoadSegmentProvisionalChange(segment.Id, out var actual));
        Assert.Equal(expected, actual, new TranslatedChangeEqualityComparer());
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
    public void TranslateWithRecordsForAddRoadSegmentReturnsExpectedResult()
    {
        var segment = _fixture.Create<AddRoadSegment>();
        var records = _fixture
            .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WB_OIDN.Value = index + 1;
                record.WS_OIDN.Value = segment.TemporaryId;
                record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                return record;
            })
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();
        var changes = TranslatedChanges.Empty.AppendChange(segment);

        var result = _sut.Translate(_entry, enumerator, changes);

        var expected =
            TranslatedChanges.Empty.AppendChange(
                records
                    .Where(record => record.RECORDTYPE.Value != (short)RecordType.Removed.Translation.Identifier)
                    .Aggregate(
                        segment,
                        (current, record) => current.WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(record.WB_OIDN.Value),
                                new RoadSegmentWidth(record.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value)))
                        )
                    )
            );

        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }

    [Fact]
    public void TranslateWithRecordsForModifyRoadSegmentReturnsExpectedResult()
    {
        var segment = _fixture.Create<ModifyRoadSegment>();
        var records = _fixture
            .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WB_OIDN.Value = index + 1;
                record.WS_OIDN.Value = segment.Id;
                record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                return record;
            })
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();
        var changes = TranslatedChanges.Empty.AppendChange(segment);

        var result = _sut.Translate(_entry, enumerator, changes);

        var expected =
            TranslatedChanges.Empty.AppendChange(
                records
                    .Where(record => record.RECORDTYPE.Value != (short)RecordType.Removed.Translation.Identifier)
                    .Aggregate(
                        segment,
                        (current, record) => current.WithWidth(
                            new RoadSegmentWidthAttribute(
                                new AttributeId(record.WB_OIDN.Value),
                                new RoadSegmentWidth(record.BREEDTE.Value),
                                new RoadSegmentPosition(Convert.ToDecimal(record.VANPOSITIE.Value)),
                                new RoadSegmentPosition(Convert.ToDecimal(record.TOTPOSITIE.Value)))
                        )
                    )
            );

        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }

    [Fact]
    public void TranslateWithRecordsForRemoveRoadSegmentReturnsExpectedResult()
    {
        var segment = _fixture.Create<RemoveRoadSegment>();
        var records = _fixture
            .CreateMany<RoadSegmentWidthChangeDbaseRecord>(new Random().Next(1, 5))
            .Select((record, index) =>
            {
                record.WB_OIDN.Value = index + 1;
                record.WS_OIDN.Value = segment.Id;
                record.TOTPOSITIE.Value = record.VANPOSITIE.Value + 1.0;
                return record;
            })
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();
        var changes = TranslatedChanges.Empty.AppendChange(segment);

        var result = _sut.Translate(_entry, enumerator, changes);

        var expected = TranslatedChanges.Empty.AppendChange(segment);

        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }
}
