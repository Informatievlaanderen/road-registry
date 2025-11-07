namespace RoadRegistry.Projections.Tests.Projections.Template;

using AutoFixture;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class RoadSegmentProjectionTests
{
    [Fact]
    public Task WhenRoadSegmentAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;

        var roadSegment1Added = fixture.Create<RoadSegmentAdded>();
        var roadSegment2Added = fixture.Create<RoadSegmentAdded>();

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

        return BuildProjection()
            .Scenario()
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

        await BuildProjection()
            .Scenario()
            .Given(roadSegment1Added, roadSegment1Removed)
            .ExpectNone();
    }

    private RoadSegmentProjection BuildProjection()
    {
        return new RoadSegmentProjection();
    }
}
