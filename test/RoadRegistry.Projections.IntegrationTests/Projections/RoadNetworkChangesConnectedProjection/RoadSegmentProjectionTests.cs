namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using AutoFixture;
using RoadRegistry.Projections.IntegrationTests.Infrastructure;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment.Events.V2;

[Collection(nameof(DockerFixtureCollection))]
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

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment2Added = fixture.Create<RoadSegmentWasAdded>();

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
        fixture.Freeze<RoadSegmentId>();

        var roadSegment1Added = fixture.Create<RoadSegmentWasAdded>();
        var roadSegment1Removed = fixture.Create<RoadSegmentWasRemoved>();

        await CreateProjectionTestRunner()
            .Given(roadSegment1Added, roadSegment1Removed)
            .ExpectNone(roadSegment1Added.RoadSegmentId);
    }

    private MartenProjectionIntegrationTestRunner CreateProjectionTestRunner()
    {
        return new MartenProjectionIntegrationTestRunner(_databaseFixture)
            .ConfigureRoadNetworkChangesProjection<RoadSegmentProjection>(RoadSegmentProjection.Configure);
    }
}
