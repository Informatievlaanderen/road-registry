namespace RoadRegistry.SyncHost.Tests.StreetName.Projections;

using Fixtures;
using RoadRegistry.StreetName;
using RoadRegistry.StreetNameConsumer.Projections;
using RoadRegistry.StreetNameConsumer.Schema;

public class StreetNameConsumerProjectionTests : IClassFixture<StreetNameConsumerProjectionFixture>
{
    private readonly StreetNameConsumerProjectionFixture _fixture;

    public StreetNameConsumerProjectionTests(StreetNameConsumerProjectionFixture fixture)
    {
        _fixture = fixture;
    }

    private StreetNameConsumerItem? Current => _fixture.GetConsumerItem(_fixture.StreetName1.Identificator.Id);

    [Fact]
    public async Task SnapshotAddedNewStreetName()
    {
        var record = _fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await _fixture.ProjectAsync(new StreetNameSnapshotOsloWasProduced
        {
            StreetNameId = record.Identificator.Id,
            Record = record
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
    public async Task SnapshotUpdatedStreetName()
    {
        var record = _fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await _fixture.ProjectAsync(new StreetNameSnapshotOsloWasProduced
        {
            StreetNameId = _fixture.StreetName1.Identificator.Id,
            Record = record
        });

        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);

        record.StraatnaamStatus = StreetNameStatus.Retired;

        await _fixture.ProjectAsync(new StreetNameSnapshotOsloWasProduced
        {
            StreetNameId = record.Identificator.Id,
            Record = record
        });

        Assert.Equal(record.Identificator.Id, Current.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);
    }

    [Fact]
    public async Task SnapshotRemovedStreetName()
    {
        var record = _fixture.StreetName1;
        record.StraatnaamStatus = StreetNameStatus.Current;

        await _fixture.ProjectAsync(new StreetNameSnapshotOsloWasProduced
        {
            StreetNameId = _fixture.StreetName1.Identificator.Id,
            Record = record
        });

        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);

        await _fixture.ProjectAsync(new StreetNameSnapshotOsloWasProduced
        {
            StreetNameId = _fixture.StreetName1.Identificator.Id,
            Record = new StreetNameSnapshotOsloRecord()
        });

        Assert.Equal(record.Identificator.Id, Current.StreetNameId);
        Assert.Equal(record.StraatnaamStatus, Current.StreetNameStatus);
        Assert.True(Current.IsRemoved);
    }
}
