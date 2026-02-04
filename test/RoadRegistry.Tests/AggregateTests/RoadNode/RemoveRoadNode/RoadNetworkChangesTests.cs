namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNode;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNode.Changes;
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
    public void ThenRoadNodeIdIsRegistered()
    {
        var change = _fixture.Create<RemoveRoadNodeChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.RoadNodeIds.Should().Contain(change.RoadNodeId);

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().BeNull();
    }
}
