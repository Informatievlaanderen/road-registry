namespace RoadRegistry.Tests.AggregateTests.RoadNode.AddRoadNode;

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
    public void ThenGeometryIsUsedInScope()
    {
        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<AddRoadNodeChange>());

        changes.RoadNodeIds.Should().BeEmpty();

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().NotBeEmpty();
    }
}
