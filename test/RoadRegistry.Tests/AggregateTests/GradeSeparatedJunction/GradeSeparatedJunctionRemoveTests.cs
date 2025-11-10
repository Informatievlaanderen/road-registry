namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Events;

public class GradeSeparatedJunctionRemoveTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionRemoved()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var junction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionAdded>())
            .WithoutChanges();

        // Act
        var problems = junction.Remove();

        // Assert
        problems.HasError().Should().BeFalse();
        junction.GetChanges().Should().HaveCount(1);

        var junctionRemoved = (GradeSeparatedJunctionRemoved)junction.GetChanges().Single();
        junctionRemoved.GradeSeparatedJunctionId.Should().Be(junction.GradeSeparatedJunctionId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var junctionAdded = Fixture.Create<GradeSeparatedJunctionAdded>();
        var junction = GradeSeparatedJunction.Create(junctionAdded);

        var evt = Fixture.Create<GradeSeparatedJunctionRemoved>();

        // Act
        junction.Apply(evt);

        // Assert
        junction.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        junction.IsRemoved.Should().BeTrue();
        junction.Type.Should().Be(junctionAdded.Type);
        junction.LowerRoadSegmentId.Should().Be(junctionAdded.LowerRoadSegmentId);
        junction.UpperRoadSegmentId.Should().Be(junctionAdded.UpperRoadSegmentId);
    }
}
