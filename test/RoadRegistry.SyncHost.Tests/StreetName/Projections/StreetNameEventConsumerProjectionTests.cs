namespace RoadRegistry.SyncHost.Tests.StreetName.Projections;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Fixtures;
using Microsoft.EntityFrameworkCore;
using Sync.StreetNameRegistry;

public class StreetNameEventConsumerProjectionTests : IClassFixture<StreetNameEventConsumerProjectionFixture>
{
    private readonly IDbContextFactory<StreetNameEventConsumerContext> _dbContextFactory;

    public StreetNameEventConsumerProjectionTests(IDbContextFactory<StreetNameEventConsumerContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private async Task<StreetNameEventConsumerProjectionFixture> BuildFixture()
    {
        return new StreetNameEventConsumerProjectionFixture(await _dbContextFactory.CreateDbContextAsync());
    }

    [Fact]
    public async Task StreetNameRenamed()
    {
        var fixture = await BuildFixture();

        var streetNameLocalId1 = 1;
        var streetNameLocalId2 = 2;

        await fixture.ProjectAsync(new StreetNameWasRenamed(string.Empty, streetNameLocalId1, streetNameLocalId2, new FakeProvenance()));

        var actual = fixture.GetRenamedStreetNameRecord(streetNameLocalId1);

        Assert.Equal(streetNameLocalId1, actual.StreetNameLocalId);
        Assert.Equal(streetNameLocalId2, actual.DestinationStreetNameLocalId);
    }
}
