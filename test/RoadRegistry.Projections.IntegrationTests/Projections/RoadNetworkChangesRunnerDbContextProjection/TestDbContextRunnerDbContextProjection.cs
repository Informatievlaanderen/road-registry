namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Microsoft.EntityFrameworkCore;

public class TestDbContextRunnerDbContextProjection : RoadRegistry.Infrastructure.MartenDb.Projections.RoadNetworkChangesRunnerDbContextProjection<TestDbContext>
{
    public TestDbContextRunnerDbContextProjection(
        ICollection<ConnectedProjection<TestDbContext>> projections,
        IDbContextFactory<TestDbContext> dbContextFactory)
        : base(Resolve.WhenAssignableToHandlerMessageType(projections.SelectMany(x => x.Handlers).ToArray()), dbContextFactory)
    {
    }
}
