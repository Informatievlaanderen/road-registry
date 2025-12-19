namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegmentToNationalRoad;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
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
        var change = _fixture.Create<AddRoadSegmentToNationalRoadChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.RoadSegmentIds.Should().Contain(change.RoadSegmentId);

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }

    [Fact]
    public void WhenRoadSegmentIsAdded_ThenRoadSegmentIdIsNotRegistered()
    {
        _fixture.Freeze<RoadSegmentId>();

        var change = _fixture.Create<AddRoadSegmentToNationalRoadChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<AddRoadSegmentChange>())
            .Add(change);

        changes.RoadSegmentIds.Should().NotContain(change.RoadSegmentId);
    }
}
