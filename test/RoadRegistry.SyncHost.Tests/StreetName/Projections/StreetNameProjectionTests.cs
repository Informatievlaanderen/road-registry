namespace RoadRegistry.SyncHost.Tests.StreetName.Projections;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Fixtures;
using RoadRegistry.StreetName;
using StreetNameRecord = Sync.StreetNameRegistry.StreetNameRecord;

public class StreetNameProjectionTests : IClassFixture<StreetNameProjectionFixture>
{
    private readonly StreetNameProjectionFixture _fixture;

    public StreetNameProjectionTests(StreetNameProjectionFixture fixture)
    {
        _fixture = fixture;
    }

    private StreetNameRecord? Current => _fixture.GetStreetNameRecord(_fixture.StreetName1.Identificator.Id);

    [Fact]
    public async Task StreetNameAdded()
    {
        var record = _fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await _fixture.ProjectAsync(new StreetNameCreated
        {
            Record = ToStreetNameRecord(record)
        });

        Assert.Equal(record.Identificator.Id, Current.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);
        Assert.Equal("Straat", Current.DutchName);
        Assert.Equal("Rue", Current.FrenchName);
        Assert.Equal("Street", Current.EnglishName);
        Assert.Equal("Strasse", Current.GermanName);
        Assert.Equal("NL", Current.DutchHomonymAddition);
        Assert.Equal("FR", Current.FrenchHomonymAddition);
        Assert.Equal("EN", Current.EnglishHomonymAddition);
        Assert.Equal("DE", Current.GermanHomonymAddition);
    }

    [Fact]
    public async Task StreetNameModified()
    {
        var record = _fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await _fixture.ProjectAsync(new StreetNameCreated
        {
            Record = ToStreetNameRecord(record)
        });

        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);

        record.StraatnaamStatus = StreetNameStatus.Retired;

        await _fixture.ProjectAsync(new StreetNameModified
        {
            Record = ToStreetNameRecord(record),
            StatusModified = true
        });

        Assert.Equal(record.Identificator.Id, Current.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);
    }

    [Fact]
    public async Task StreetNameRemoved()
    {
        var record = _fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await _fixture.ProjectAsync(new StreetNameCreated
        {
            Record = ToStreetNameRecord(record)
        });

        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);

        await _fixture.ProjectAsync(new StreetNameRemoved
        {
            StreetNameId = _fixture.StreetName1.Identificator.Id
        });

        Assert.Equal(record.Identificator.Id, Current.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);
        Assert.True(Current.IsRemoved);
    }

    private static BackOffice.Messages.StreetNameRecord ToStreetNameRecord(StreetNameSnapshotOsloRecord snapshotRecord)
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
