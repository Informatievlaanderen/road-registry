namespace RoadRegistry.SyncHost.Tests.StreetName.Projections;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Fixtures;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.StreetName;
using Sync.StreetNameRegistry;

public class StreetNameSnapshotProjectionTests : IClassFixture<StreetNameSnapshotProjectionFixture>
{
    private readonly IDbContextFactory<StreetNameSnapshotProjectionContext> _dbContextFactory;

    public StreetNameSnapshotProjectionTests(IDbContextFactory<StreetNameSnapshotProjectionContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private async Task<StreetNameSnapshotProjectionFixture> BuildFixture()
    {
        return new StreetNameSnapshotProjectionFixture(await _dbContextFactory.CreateDbContextAsync());
    }

    [Fact]
    public async Task StreetNameAdded()
    {
        var fixture = await BuildFixture();

        var record = fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await fixture.ProjectEnvelopeAsync(new StreetNameCreated
        {
            Record = ToStreetNameRecord(record)
        });

        var actual = fixture.GetStreetNameRecord(fixture.StreetName1.Identificator.Id);

        Assert.Equal(record.Identificator.Id, actual.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, actual.StreetNameStatus);
        Assert.Equal("Straat", actual.DutchName);
        Assert.Equal("Rue", actual.FrenchName);
        Assert.Equal("Street", actual.EnglishName);
        Assert.Equal("Strasse", actual.GermanName);
        Assert.Equal("NL", actual.DutchHomonymAddition);
        Assert.Equal("FR", actual.FrenchHomonymAddition);
        Assert.Equal("EN", actual.EnglishHomonymAddition);
        Assert.Equal("DE", actual.GermanHomonymAddition);
    }

    [Fact]
    public async Task StreetNameModified()
    {
        var fixture = await BuildFixture();

        var record = fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await fixture.ProjectEnvelopeAsync(new StreetNameCreated
        {
            Record = ToStreetNameRecord(record)
        });

        var actual = fixture.GetStreetNameRecord(fixture.StreetName1.Identificator.Id);
        Assert.Equal(record.StraatnaamStatus, actual.StreetNameStatus);

        record.StraatnaamStatus = StreetNameStatus.Retired;

        await fixture.ProjectEnvelopeAsync(new StreetNameModified
        {
            Record = ToStreetNameRecord(record),
            StatusModified = true
        });

        actual = fixture.GetStreetNameRecord(fixture.StreetName1.Identificator.Id);
        Assert.Equal(record.Identificator.Id, actual.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, actual.StreetNameStatus);
    }

    [Fact]
    public async Task StreetNameRemoved()
    {
        var fixture = await BuildFixture();

        var record = fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await fixture.ProjectEnvelopeAsync(new StreetNameCreated
        {
            Record = ToStreetNameRecord(record)
        });

        var actual = fixture.GetStreetNameRecord(fixture.StreetName1.Identificator.Id);
        Assert.Equal(record.StraatnaamStatus, actual.StreetNameStatus);

        await fixture.ProjectEnvelopeAsync(new StreetNameRemoved
        {
            StreetNameId = fixture.StreetName1.Identificator.Id
        });

        actual = fixture.GetStreetNameRecord(fixture.StreetName1.Identificator.Id);
        Assert.Equal(record.Identificator.Id, actual.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, actual.StreetNameStatus);
        Assert.True(actual.IsRemoved);
    }

    private static BackOffice.Messages.StreetNameRecord ToStreetNameRecord(StreetNameSnapshotRecord snapshotRecord)
    {
        var streetNameId = StreetNamePuri.FromValue(snapshotRecord.Identificator.Id);
        var streetNameLocalId = streetNameId.ToStreetNameLocalId();

        return new BackOffice.Messages.StreetNameRecord
        {
            StreetNameId = streetNameId,
            PersistentLocalId = streetNameLocalId,
            NisCode = snapshotRecord?.Gemeente.ObjectId,

            DutchName = GetSpelling(snapshotRecord?.Straatnamen, Taal.NL),
            FrenchName = GetSpelling(snapshotRecord?.Straatnamen, Taal.FR),
            GermanName = GetSpelling(snapshotRecord?.Straatnamen, Taal.DE),
            EnglishName = GetSpelling(snapshotRecord?.Straatnamen, Taal.EN),

            DutchHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.NL),
            FrenchHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.FR),
            GermanHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.DE),
            EnglishHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.EN),

            StreetNameStatus = snapshotRecord?.StraatnaamStatus
        };
    }

    private static string? GetSpelling(List<DeseriazableGeografischeNaam>? namen, Taal taal)
    {
        return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
    }
}
