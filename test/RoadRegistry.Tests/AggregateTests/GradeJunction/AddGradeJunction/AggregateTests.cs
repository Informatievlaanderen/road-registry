namespace RoadRegistry.Tests.AggregateTests.GradeJunction.AddGradeJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Events.V2;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<GradeJunctionWasAdded>();

        // Act
        var junction = GradeJunction.Create(evt);

        // Assert
        junction.GradeJunctionId.Should().Be(evt.GradeJunctionId);
        junction.RoadSegmentId1.Should().Be(evt.RoadSegmentId1);
        junction.RoadSegmentId2.Should().Be(evt.RoadSegmentId2);
    }
}
