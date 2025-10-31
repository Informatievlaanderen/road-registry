namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections;

using AutoFixture;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadSegment;
using RoadSegment.Events;
using RoadSegment.ValueObjects;
using Tests.BackOffice.Scenarios;

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
            .Given<RoadSegment, RoadSegmentId>([
                (roadSegment1Added.Id, [roadSegment1Added]),
                (roadSegment2Added.Id, [roadSegment2Added]),
            ])
            .Expect([
                (roadSegment1Added.Id, expectedRoadSegment1),
                (roadSegment2Added.Id, expectedRoadSegment2),
            ]);
    }

    private MartenProjectionTestRunner CreateProjectionTestRunner()
    {
        return new MartenProjectionTestRunner(_databaseFixture)
            .ConfigureRoadNetworkChangesProjection<RoadSegmentProjection>(RoadSegmentProjection.Configure);
    }
}
