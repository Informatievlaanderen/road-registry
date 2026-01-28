namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
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
            .Add(_fixture.Create<AddRoadSegmentChange>());

        changes.Ids.RoadSegmentIds.Should().BeEmpty();

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().NotBeEmpty();
    }
}
