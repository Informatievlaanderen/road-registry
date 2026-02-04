namespace RoadRegistry.Tests.AggregateTests.RoadNode.AddRoadNode;

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
    public void ThenGeometryIsUsedInScope()
    {
        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<AddRoadNodeChange>());

        changes.Ids.RoadNodeIds.Should().BeEmpty();

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().NotBeNull();
    }
}
