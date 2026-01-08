namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNode;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.Tests.AggregateTests;
using ScopedRoadNetwork;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestData().Fixture;
    }

    [Fact]
    public void ThenRoadNodeIdIsRegistered()
    {
        var change = _fixture.Create<RemoveRoadNodeChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.RoadNodeIds.Should().Contain(change.RoadNodeId);

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }
}
