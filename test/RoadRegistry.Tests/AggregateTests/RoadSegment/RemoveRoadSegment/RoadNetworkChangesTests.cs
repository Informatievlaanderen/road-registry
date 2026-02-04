namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadNetwork;
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
    public void ThenRoadSegmentIdIsRegistered()
    {
        var change = _fixture.Create<RemoveRoadSegmentChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.RoadSegmentIds.Should().Contain(change.RoadSegmentId);

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().BeNull();
    }
}
