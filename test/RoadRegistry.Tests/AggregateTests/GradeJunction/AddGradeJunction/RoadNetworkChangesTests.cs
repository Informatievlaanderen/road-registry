namespace RoadRegistry.Tests.AggregateTests.GradeJunction.AddGradeJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestDataV2().Fixture;
    }

    [Fact]
    public void WhenRoadSegmentIsNotAdded_ThenRoadSegmentIdsAreRegistered()
    {
        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<AddGradeJunctionChange>());

        changes.Ids.RoadSegmentIds.Should().NotBeEmpty();

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().BeNull();
    }

    [Fact]
    public void WhenRoadSegmentIsAdded_ThenRoadSegmentIdsAreNotRegistered()
    {
        _fixture.Freeze<RoadSegmentId>();

        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<AddRoadSegmentChange>())
            .Add(_fixture.Create<AddGradeJunctionChange>());

        changes.Ids.RoadSegmentIds.Should().BeEmpty();
    }
}
