namespace RoadRegistry.Tests.AggregateTests.RoadNetwork;

using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork;

public class RoadNetworkChangesTests
{
    [Fact]
    public void StartShouldBeEmpty()
    {
        var changes = RoadNetworkChanges.Start();

        changes.Should().BeEmpty();
        changes.Ids.RoadNodeIds.Should().BeEmpty();
        changes.Ids.RoadSegmentIds.Should().BeEmpty();
        changes.Ids.GradeSeparatedJunctionIds.Should().BeEmpty();

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }
}
