namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// A RunnerDbContext-backed sub-projection built from a set of ConnectedProjection handlers over the TestDbContext.
public class TestDbContextRunnerDbContextProjection : RunnerDbContextRoadNetworkChangesProjection<TestDbContext>
{
    public TestDbContextRunnerDbContextProjection(ICollection<ConnectedProjection<TestDbContext>> projections)
        : base(Resolve.WhenAssignableToHandlerMessageType(projections.SelectMany(x => x.Handlers).ToArray()))
    {
    }
}

// The DbContext-backed driver that owns the TestDbContext factory and the single projection state, dispatching each
// event to its sub-projections.
public class TestDbContextRoadNetworkChangesProjection : DbContextBackedRoadNetworkChangesProjection<TestDbContext>
{
    public TestDbContextRoadNetworkChangesProjection(
        IDbContextFactory<TestDbContext> dbContextFactory,
        IReadOnlyCollection<IRoadNetworkChangesProjection<TestDbContext>> projections)
        : base(dbContextFactory, projections, NullLoggerFactory.Instance)
    {
    }
}
