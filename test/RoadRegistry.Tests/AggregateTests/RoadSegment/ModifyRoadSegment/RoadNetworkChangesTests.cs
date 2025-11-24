namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadNetwork;
using RoadRegistry.RoadSegment.Changes;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestData().Fixture;
    }

    [Fact]
    public void ThenRoadSegmentIdIsRegisteredAndGeometryIsUsedInScope()
    {
        var change = _fixture.Create<ModifyRoadSegmentChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.RoadSegmentIds.Should().Contain(change.RoadSegmentId);

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().NotBeEmpty();
    }
}
