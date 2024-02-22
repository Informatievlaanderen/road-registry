namespace RoadRegistry.SyncHost.Tests.StreetName.Projections;

using Fixtures;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice.Messages;
using Sync.StreetNameRegistry;

public class StreetNameEventProjectionTests : IClassFixture<StreetNameEventProjectionFixture>
{
    private readonly IDbContextFactory<StreetNameEventProjectionContext> _dbContextFactory;

    public StreetNameEventProjectionTests(IDbContextFactory<StreetNameEventProjectionContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private async Task<StreetNameEventProjectionFixture> BuildFixture()
    {
        return new StreetNameEventProjectionFixture(await _dbContextFactory.CreateDbContextAsync());
    }

    [Fact]
    public async Task StreetNameRenamed()
    {
        var fixture = await BuildFixture();

        var streetNameLocalId1 = 1;
        var streetNameLocalId2 = 2;

        await fixture.ProjectEnvelopeAsync(new StreetNameRenamed
        {
            StreetNameLocalId = streetNameLocalId1,
            DestinationStreetNameLocalId = streetNameLocalId2
        });

        var actual = fixture.GetRenamedStreetNameRecord(streetNameLocalId1);

        Assert.Equal(streetNameLocalId1, actual.StreetNameLocalId);
        Assert.Equal(streetNameLocalId2, actual.DestinationStreetNameLocalId);
    }
}
