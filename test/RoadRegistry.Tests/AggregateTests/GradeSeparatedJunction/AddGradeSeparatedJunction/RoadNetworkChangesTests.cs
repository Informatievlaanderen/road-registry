namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.AddGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using ScopedRoadNetwork;

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
            .Add(_fixture.Create<AddGradeSeparatedJunctionChange>());

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
            .Add(_fixture.Create<AddGradeSeparatedJunctionChange>());

        changes.Ids.RoadSegmentIds.Should().BeEmpty();
    }
}
