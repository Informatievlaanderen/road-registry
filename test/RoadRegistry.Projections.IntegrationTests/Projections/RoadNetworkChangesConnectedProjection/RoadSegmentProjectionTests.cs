namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using AutoFixture;
using RoadRegistry.Projections.IntegrationTests.Infrastructure;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment.Events.V2;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class RoadSegmentProjectionTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public RoadSegmentProjectionTests(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task WhenRoadSegmentAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>()
            with
            {
                RoadSegmentId = new RoadSegmentId(1)
            };
        var roadSegment2Added = fixture.Create<RoadSegmentWasAdded>()
            with
            {
                RoadSegmentId = new RoadSegmentId(2)
            };

        var expectedRoadSegment1 = new RoadSegmentProjectionItem
        {
            Id = roadSegment1Added.RoadSegmentId,
            GeometryDrawMethod = roadSegment1Added.GeometryDrawMethod
        };
        var expectedRoadSegment2 = new RoadSegmentProjectionItem
        {
            Id = roadSegment2Added.RoadSegmentId,
            GeometryDrawMethod = roadSegment2Added.GeometryDrawMethod
        };

        await CreateProjectionTestRunner()
            //.ProjectionWaitTimeout(TimeSpan.FromMinutes(10))
            .Given(roadSegment1Added, roadSegment2Added)
            .Expect(expectedRoadSegment1, expectedRoadSegment2);
    }

    [Fact]
    public async Task WhenRoadSegmentRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;
        var id = new RoadSegmentId(3);

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>()
            with
            {
                RoadSegmentId = id
            };
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRemoved>()
            with
            {
                RoadSegmentId = id
            };

        await CreateProjectionTestRunner()
            .Given(roadSegment1Added, roadSegment1Removed)
            .ExpectNone(roadSegment1Added.RoadSegmentId);
    }

    private MartenProjectionIntegrationTestRunner CreateProjectionTestRunner()
    {
        var logger = _testOutputHelper.ToLogger<RoadSegmentProjection>();
        return new MartenProjectionIntegrationTestRunner(_databaseFixture, logger)
            .ConfigureRoadNetworkChangesProjection([
                new RoadSegmentProjection(logger)
            ], RoadSegmentProjection.Configure, logger);
    }
}
