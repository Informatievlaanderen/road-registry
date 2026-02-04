namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.RemoveGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNetwork;
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
        var change = _fixture.Create<RemoveGradeSeparatedJunctionChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.Ids.GradeSeparatedJunctionIds.Should().Contain(change.GradeSeparatedJunctionId);

        var scope = changes.BuildScopeGeometry();
        ((IComparable<Geometry>)scope).Should().BeNull();
    }
}
