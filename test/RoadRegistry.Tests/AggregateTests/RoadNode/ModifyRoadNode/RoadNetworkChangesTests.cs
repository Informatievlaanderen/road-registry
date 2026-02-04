namespace RoadRegistry.Tests.AggregateTests.RoadNode.ModifyRoadNode;

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
    public void ThenRoadNodeIdIsRegisteredAndGeometryIsUsedInScope()
    {
        var change = _fixture.Create<ModifyRoadNodeChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.RoadNodeIds.Should().Contain(change.RoadNodeId);

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().NotBeNull();
    }
}
