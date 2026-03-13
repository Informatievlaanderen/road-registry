namespace RoadRegistry.Tests.AggregateTests.GradeJunction.RemoveGradeJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction.Changes;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.AggregateTests;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestDataV2().Fixture;
    }

    [Fact]
    public void ThenGradeJunctionIdIsRegistered()
    {
        var change = _fixture.Create<RemoveGradeJunctionChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.GradeJunctionIds.Should().Contain(change.GradeJunctionId);

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().BeNull();
    }
}
