namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegmentToEuropeanRoad;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests;
using ScopedRoadNetwork;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestDataV2().Fixture;
    }

    [Fact]
    public void ThenRoadSegmentIdIsRegistered()
    {
        var change = _fixture.Create<AddRoadSegmentToEuropeanRoadChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.RoadSegmentIds.Should().Contain(change.RoadSegmentId);

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }

    [Fact]
    public void WhenRoadSegmentIsAdded_ThenRoadSegmentIdIsNotRegistered()
    {
        _fixture.Freeze<RoadSegmentId>();

        var change = _fixture.Create<AddRoadSegmentToEuropeanRoadChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<AddRoadSegmentChange>())
            .Add(change);

        changes.Ids.RoadSegmentIds.Should().NotContain(change.RoadSegmentId);
    }
}
