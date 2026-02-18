namespace RoadRegistry.Tests.AggregateTests.RoadNetwork;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
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
        ((IComparable<Geometry>)scope).Should().BeNull();
    }

    [Fact]
    public void AllChangeShouldHaveAnOrderSpecified()
    {
        var roadNetworkChangeTypes = typeof(IRoadNetworkChange).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(IRoadNetworkChange)) && x != typeof(IRoadNetworkChange))
            .ToArray();
        roadNetworkChangeTypes.Should().NotBeEmpty();

        foreach (var roadNetworkChangeType in roadNetworkChangeTypes)
        {
            RoadNetworkChanges.ChangeOrderTypes.Should().Contain(roadNetworkChangeType);
        }

        var fixture = new RoadNetworkTestDataV2().Fixture;
        var changes = RoadNetworkChanges.Start()
            .Add(fixture.Create<AddRoadSegmentChange>())
            .Add(fixture.Create<AddRoadNodeChange>());

        var orderedChanges = changes.ToList();
        orderedChanges[0].GetType().Should().Be(typeof(AddRoadNodeChange));
        orderedChanges[1].GetType().Should().Be(typeof(AddRoadSegmentChange));
    }
}
