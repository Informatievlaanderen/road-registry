namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class TestDbContext : RunnerDbContext<TestDbContext>
{
    public override string ProjectionStateSchema => "test";

    public DbSet<RoadSegmentRecord> RoadSegments => Set<RoadSegmentRecord>();

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }
}

public sealed record RoadSegmentRecord(int Id);
