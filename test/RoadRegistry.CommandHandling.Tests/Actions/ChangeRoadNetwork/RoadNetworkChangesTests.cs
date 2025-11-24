namespace RoadRegistry.CommandHandling.Tests.Actions.ChangeRoadNetwork;

using System.Linq;
using FluentAssertions;
using RoadNetwork;

public class RoadNetworkChangesTests
{
    [Fact]
    public void StartShouldBeEmpty()
    {
        var changes = RoadNetworkChanges.Start();

        changes.Should().BeEmpty();
        changes.RoadNodeIds.Should().BeEmpty();
        changes.RoadSegmentIds.Should().BeEmpty();
        changes.GradeSeparatedJunctionIds.Should().BeEmpty();

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }
}
