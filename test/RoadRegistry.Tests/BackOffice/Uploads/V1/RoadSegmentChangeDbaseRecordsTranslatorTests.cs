namespace RoadRegistry.Tests.BackOffice.Uploads.V1;

using System.IO.Compression;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Polly;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Schema;
using RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V1.Validation;

public class RoadSegmentChangeDbaseRecordsTranslatorTests : IDisposable
{
    private readonly ZipArchive _archive;
    private readonly ZipArchiveEntry _entry;
    private readonly IDbaseRecordEnumerator<RoadSegmentChangeDbaseRecord> _enumerator;
    private readonly Fixture _fixture;
    private readonly MemoryStream _stream;
    private readonly RoadSegmentChangeDbaseRecordsTranslator _sut;

    public RoadSegmentChangeDbaseRecordsTranslatorTests()
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
        _fixture.Customize<RoadSegmentChangeDbaseRecord>(
            composer => composer
                .FromFactory(random => new RoadSegmentChangeDbaseRecord
                {
                    RECORDTYPE = { Value = (short)_fixture.Create<RecordType>().Translation.Identifier },
                    TRANSACTID = { Value = (short)random.Next(1, 9999) },
                    WS_OIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) },
                    METHODE = { Value = (short)_fixture.Create<RoadSegmentGeometryDrawMethod>().Translation.Identifier },
                    BEHEERDER = { Value = _fixture.Create<OrganizationId>() },
                    MORFOLOGIE = { Value = (short)_fixture.Create<RoadSegmentMorphology>().Translation.Identifier },
                    STATUS = { Value = _fixture.Create<RoadSegmentStatus>().Translation.Identifier },
                    CATEGORIE = { Value = _fixture.Create<RoadSegmentCategory>().Translation.Identifier },
                    B_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    E_WK_OIDN = { Value = new RoadNodeId(random.Next(1, int.MaxValue)) },
                    LSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                    RSTRNMID = { Value = new StreetNameLocalId(random.Next(1, int.MaxValue)) },
                    TGBEP = { Value = (short)_fixture.Create<RoadSegmentAccessRestriction>().Translation.Identifier },
                    EVENTIDN = { Value = new RoadSegmentId(random.Next(1, int.MaxValue)) }
                })
                .OmitAutoProperties());

        _sut = new RoadSegmentChangeDbaseRecordsTranslator();
        _enumerator = new List<RoadSegmentChangeDbaseRecord>().ToDbaseRecordEnumerator();
        _stream = new MemoryStream();
        _archive = new ZipArchive(_stream, ZipArchiveMode.Create);
        _entry = _archive.CreateEntry("wegsegment_all.dbf");
    }

    public void Dispose()
    {
        _archive?.Dispose();
        _stream?.Dispose();
    }

    [Fact]
    public void IsZipArchiveDbaseRecordsTranslator()
    {
        Assert.IsAssignableFrom<IZipArchiveDbaseRecordsTranslator<RoadSegmentChangeDbaseRecord>>(_sut);
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
    public void TranslateWithIdenticalRecordsReturnsExpectedResult()
    {
        var records = _fixture
            .CreateMany<RoadSegmentChangeDbaseRecord>(1)
            .Select((record, index) =>
            {
                record.WS_OIDN.Value = index + 1;
                record.RECORDTYPE.Value = (short)RecordType.Identical.Translation.Identifier;
                return record;
            })
            .ToArray();
        var enumerator = records.ToDbaseRecordEnumerator();

        var result = _sut.Translate(_entry, enumerator, TranslatedChanges.Empty);

        foreach (var current in records)
        {
            var id = new RoadSegmentId(current.WS_OIDN.Value);
            Assert.True(result.TryFindRoadSegmentProvisionalChange(id, out var foundChange));
            Assert.NotNull(foundChange);
            var actual = Assert.IsAssignableFrom<ITranslatedChange>(foundChange);
            ITranslatedChange expected = new ModifyRoadSegment(
                new RecordNumber(Array.IndexOf(records, current) + 1),
                new RoadSegmentId(current.WS_OIDN.Value),
                new RoadNodeId(current.B_WK_OIDN.Value),
                new RoadNodeId(current.E_WK_OIDN.Value),
                new OrganizationId(current.BEHEERDER.Value),
                RoadSegmentGeometryDrawMethod.ByIdentifier[current.METHODE.Value],
                RoadSegmentMorphology.ByIdentifier[current.MORFOLOGIE.Value],
                RoadSegmentStatus.ByIdentifier[current.STATUS.Value],
                RoadSegmentCategory.ByIdentifier[current.CATEGORIE.Value],
                RoadSegmentAccessRestriction.ByIdentifier[current.TGBEP.Value],
                current.LSTRNMID.Value.HasValue
                    ? new StreetNameLocalId(current.LSTRNMID.Value.GetValueOrDefault())
                    : default,
                current.RSTRNMID.Value.HasValue
                    ? new StreetNameLocalId(current.RSTRNMID.Value.GetValueOrDefault())
                    : default
            );

            Assert.Equal(expected, actual, new TranslatedChangeEqualityComparer());
        }
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
            .CreateMany<RoadSegmentChangeDbaseRecord>(new Random().Next(1, 4))
            .Select((record, index) =>
            {
                record.WS_OIDN.Value = index + 1;
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
                            new AddRoadSegment(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                current.EVENTIDN.HasValue && current.EVENTIDN.Value != 0
                                    ? new RoadSegmentId(current.EVENTIDN.Value)
                                    : new RoadSegmentId(current.WS_OIDN.Value),
                                new RoadSegmentId(current.WS_OIDN.Value),
                                new RoadNodeId(current.B_WK_OIDN.Value),
                                new RoadNodeId(current.E_WK_OIDN.Value),
                                new OrganizationId(current.BEHEERDER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[current.METHODE.Value],
                                RoadSegmentMorphology.ByIdentifier[current.MORFOLOGIE.Value],
                                RoadSegmentStatus.ByIdentifier[current.STATUS.Value],
                                RoadSegmentCategory.ByIdentifier[current.CATEGORIE.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[current.TGBEP.Value],
                                current.LSTRNMID.Value.HasValue
                                    ? new StreetNameLocalId(current.LSTRNMID.Value.GetValueOrDefault())
                                    : default,
                                current.RSTRNMID.Value.HasValue
                                    ? new StreetNameLocalId(current.RSTRNMID.Value.GetValueOrDefault())
                                    : default
                            )
                        );
                        break;
                    case RecordType.ModifiedIdentifier:
                        nextChanges = previousChanges.AppendChange(
                            new ModifyRoadSegment(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new RoadSegmentId(current.WS_OIDN.Value),
                                new RoadNodeId(current.B_WK_OIDN.Value),
                                new RoadNodeId(current.E_WK_OIDN.Value),
                                new OrganizationId(current.BEHEERDER.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[current.METHODE.Value],
                                RoadSegmentMorphology.ByIdentifier[current.MORFOLOGIE.Value],
                                RoadSegmentStatus.ByIdentifier[current.STATUS.Value],
                                RoadSegmentCategory.ByIdentifier[current.CATEGORIE.Value],
                                RoadSegmentAccessRestriction.ByIdentifier[current.TGBEP.Value],
                                current.LSTRNMID.Value.HasValue
                                    ? new StreetNameLocalId(current.LSTRNMID.Value.GetValueOrDefault())
                                    : default,
                                current.RSTRNMID.Value.HasValue
                                    ? new StreetNameLocalId(current.RSTRNMID.Value.GetValueOrDefault())
                                    : default
                            )
                        );
                        break;
                    case RecordType.RemovedIdentifier:
                        nextChanges = previousChanges.AppendChange(
                            new RemoveRoadSegment(
                                new RecordNumber(Array.IndexOf(records, current) + 1),
                                new RoadSegmentId(current.WS_OIDN.Value),
                                RoadSegmentGeometryDrawMethod.ByIdentifier[current.METHODE.Value]
                            )
                        );
                        break;
                }

                return nextChanges;
            });
        Assert.Equal(expected, result, new TranslatedChangeEqualityComparer());
    }
}
