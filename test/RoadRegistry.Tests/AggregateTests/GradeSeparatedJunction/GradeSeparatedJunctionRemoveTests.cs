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

        var gradeSeparatedJunction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionAdded>());

        // Act
        var problems = gradeSeparatedJunction.Remove();

        // Assert
        problems.HasError().Should().BeFalse();
        gradeSeparatedJunction.GetChanges().Should().HaveCount(2);

        var gradeSeparatedJunctionModified = (GradeSeparatedJunctionRemoved)gradeSeparatedJunction.GetChanges().Last();
        gradeSeparatedJunctionModified.GradeSeparatedJunctionId.Should().Be(gradeSeparatedJunction.GradeSeparatedJunctionId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var gradeSeparatedJunctionAdded = Fixture.Create<GradeSeparatedJunctionAdded>();
        var gradeSeparatedJunction = GradeSeparatedJunction.Create(gradeSeparatedJunctionAdded);

        var evt = Fixture.Create<GradeSeparatedJunctionRemoved>();

        // Act
        gradeSeparatedJunction.Apply(evt);

        // Assert
        gradeSeparatedJunction.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        gradeSeparatedJunction.Type.Should().Be(gradeSeparatedJunctionAdded.Type);
        gradeSeparatedJunction.LowerRoadSegmentId.Should().Be(gradeSeparatedJunctionAdded.LowerRoadSegmentId);
        gradeSeparatedJunction.UpperRoadSegmentId.Should().Be(gradeSeparatedJunctionAdded.UpperRoadSegmentId);
        gradeSeparatedJunction.IsRemoved.Should().BeTrue();
    }
}
