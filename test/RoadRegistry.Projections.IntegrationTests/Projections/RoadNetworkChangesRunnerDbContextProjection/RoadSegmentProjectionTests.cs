namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using AutoFixture;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoadRegistry.RoadSegment.Events;
using Tests.BackOffice.Scenarios;

[Collection("DockerFixtureCollection")]
public class RoadSegmentProjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public RoadSegmentProjectionTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task WhenRoadSegmentAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;

        var roadSegment1Added = fixture.Create<RoadSegmentAdded>();
        var roadSegment2Added = fixture.Create<RoadSegmentAdded>();

        var expectedRoadSegment1 = new RoadSegmentRecord(roadSegment1Added.RoadSegmentId);
        var expectedRoadSegment2 = new RoadSegmentRecord(roadSegment2Added.RoadSegmentId);

        await CreateProjectionTestRunner()
            .Given(roadSegment1Added, roadSegment2Added)
            .Expect(expectedRoadSegment1, expectedRoadSegment2);
    }

    private MartenProjectionIntegrationTestRunner CreateProjectionTestRunner()
    {
        var dbContextFactory = CreateDbContextFactory();

        return new MartenProjectionIntegrationTestRunner(_databaseFixture)
            .ConfigureServices(services => services.AddSingleton(dbContextFactory))
            .ConfigureRoadNetworkChangesProjection([
                new TestDbContextRunnerDbContextProjection(
                    [new RoadSegmentProjection()],
                    dbContextFactory
            )]);
    }

    private IDbContextFactory<TestDbContext> CreateDbContextFactory()
    {
        var database = Guid.NewGuid().ToString();

        var dbContextFactory = new Mock<IDbContextFactory<TestDbContext>>();
        dbContextFactory
            .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var builder = new DbContextOptionsBuilder<TestDbContext>()
                    .UseInMemoryDatabase(database);

                return new TestDbContext(builder.Options);
            });
        return dbContextFactory.Object;
    }
}
