namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class ProjectionExtensions
{
    public static Task Expect(
        this MartenProjectionIntegrationTestRunner runner,
        params object[] records)
    {
        return runner.Expect(async (sp, _) =>
        {
            var dbContextFactory = sp.GetRequiredService<IDbContextFactory<TestDbContext>>();
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var actualRecords = await dbContext.AllRecords();
            runner.CompareResult(records, actualRecords);
        });
    }

    private static async Task<object[]> AllRecords(this TestDbContext context)
    {
        var records = new List<object>();
        records.AddRange(await context.RoadSegments.ToArrayAsync());

        return records.ToArray();
    }
}
