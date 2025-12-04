namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.Tests.AggregateTests;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestData().Fixture;
    }

    [Fact]
    public void ThenRoadSegmentIdIsRegistered()
    {
        var change = _fixture.Create<RemoveRoadSegmentChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.RoadSegmentIds.Should().Contain(change.RoadSegmentId);

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }
}
