namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.RemoveGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNetwork;
using RoadRegistry.Tests.AggregateTests;

public class RoadNetworkChangesTests
{
    private readonly IFixture _fixture;

    public RoadNetworkChangesTests()
    {
        _fixture = new RoadNetworkTestData().Fixture;
    }

    [Fact]
    public void ThenGradeSeparatedJunctionIdIsRegistered()
    {
        var change = _fixture.Create<RemoveGradeSeparatedJunctionChange>();
        var changes = RoadNetworkChanges.Start()
            .Add(change);

        changes.GradeSeparatedJunctionIds.Should().Contain(change.GradeSeparatedJunctionId);

        var scope = changes.BuildScopeGeometry();
        scope.ToList().Should().BeEmpty();
    }
}
