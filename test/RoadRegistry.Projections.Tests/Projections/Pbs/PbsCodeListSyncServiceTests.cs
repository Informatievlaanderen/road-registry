namespace RoadRegistry.Projections.Tests.Projections.Pbs;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;
using RoadRegistry.Pbs.Schema.Records;

public class PbsCodeListSyncServiceTests
{
    [Fact]
    public async Task WhenSynced_ThenTheEnumBasedCodeListsAreFilled()
    {
        var dbContextFactory = new TestDbContextFactory();

        await Sync(dbContextFactory);

        await using var context = dbContextFactory.CreateDbContext();
        Assert.NotEmpty(await context.RoadNodeTypeCodeList.ToListAsync());
        Assert.NotEmpty(await context.RoadSegmentStatusCodeList.ToListAsync());
        Assert.NotEmpty(await context.RoadSegmentMorphologyCodeList.ToListAsync());
    }

    // The Wegbeheerder code list is the organization projection's, predefined rows included. This service seeding the
    // predefined rows as well put two writers on the same two keys at the same moment - it and the projection daemon
    // start together - and cost whichever committed second a primary key violation; on a new read model that was the
    // projection's first batch, which paused the shard. So this service does not write this list at all: it neither
    // adds the rows that are missing nor removes the ones it does not know about.
    [Fact]
    public async Task WhenSynced_ThenTheWegbeheerderCodeListIsLeftToTheProjection()
    {
        var dbContextFactory = new TestDbContextFactory();
        await using (var context = dbContextFactory.CreateDbContext())
        {
            context.RoadSegmentMaintenanceAuthorityCodeList.Add(new RoadSegmentMaintenanceAuthorityCodeListRecord
            {
                BEHEER = "00000001",
                LBLBEHEER = "Gemeente Aalst",
                OVOCODE = "OVO001"
            });
            await context.SaveChangesAsync();
        }

        await Sync(dbContextFactory);

        await using var assertContext = dbContextFactory.CreateDbContext();
        var codeList = await assertContext.RoadSegmentMaintenanceAuthorityCodeList.ToListAsync();

        var organization = Assert.Single(codeList);
        Assert.Equal("00000001", organization.BEHEER);
    }

    private static Task Sync(IDbContextFactory<PbsContext> dbContextFactory)
    {
        return new PbsCodeListSyncService(dbContextFactory, NullLogger<PbsCodeListSyncService>.Instance)
            .StartAsync(CancellationToken.None);
    }

    // Its own isolated in-memory database, shared by every context it creates - the same shape PbsProjectionScenario
    // uses, so a write through one context is visible through the next.
    private sealed class TestDbContextFactory : IDbContextFactory<PbsContext>
    {
        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _databaseName = Guid.NewGuid().ToString("N");

        public PbsContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<PbsContext>()
                .UseInMemoryDatabase(_databaseName, _root)
                .EnableSensitiveDataLogging()
                // Each test builds its own options, so across a test run EF creates more than its 20 internal service
                // providers; that is expected here and not a real leak, so silence the warning.
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                .Options;
            return new PbsContext(options);
        }
    }
}
