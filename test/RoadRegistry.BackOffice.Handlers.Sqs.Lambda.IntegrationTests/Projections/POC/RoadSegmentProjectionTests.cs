namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections.POC;

using AutoFixture;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment.ValueObjects;

//TODO-pr nog te verplaatsen, welke assembly? dit is geen lambda, enkel Marten

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

        var expectedRoadSegment1 = new RoadSegmentProjectionItem
        {
            Id = roadSegment1Added.Id,
            GeometryDrawMethod = roadSegment1Added.GeometryDrawMethod
        };
        var expectedRoadSegment2 = new RoadSegmentProjectionItem
        {
            Id = roadSegment2Added.Id,
            GeometryDrawMethod = roadSegment2Added.GeometryDrawMethod
        };

        await CreateProjectionTestRunner()
            .Given(roadSegment1Added, roadSegment2Added)
            .Expect(expectedRoadSegment1, expectedRoadSegment2);
    }

    [Fact]
    public async Task WhenRoadSegmentRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;
        fixture.Freeze<RoadSegmentId>();

        var roadSegment1Added = fixture.Create<RoadSegmentAdded>();
        var roadSegment1Removed = fixture.Create<RoadSegmentRemoved>();

        await CreateProjectionTestRunner()
            .Given(roadSegment1Added)
            .Given(roadSegment1Removed)
            .ExpectNone(roadSegment1Added.Id);
    }

    private MartenProjectionTestRunner CreateProjectionTestRunner()
    {
        return new MartenProjectionTestRunner(_databaseFixture)
            .ConfigureRoadNetworkChangesProjection<RoadSegmentProjection>(RoadSegmentProjection.Configure);
    }
}
