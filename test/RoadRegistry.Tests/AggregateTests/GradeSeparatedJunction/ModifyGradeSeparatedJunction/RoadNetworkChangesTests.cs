namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.ModifyGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadSegment.Changes;
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
    public void ThenGradeSeparatedJunctionIdIsRegistered()
    {
        var change = _fixture.Create<ModifyGradeSeparatedJunctionChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.GradeSeparatedJunctionIds.Should().Contain(change.GradeSeparatedJunctionId);

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().BeNull();
    }

    [Fact]
    public void WhenRoadSegmentIsNotAdded_ThenRoadSegmentIdsAreRegistered()
    {
        var changes = RoadNetworkChanges.Start()
            .Add(_fixture.Create<ModifyGradeSeparatedJunctionChange>());

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
            .Add(_fixture.Create<ModifyGradeSeparatedJunctionChange>());

        changes.Ids.RoadSegmentIds.Should().BeEmpty();
    }
}
