namespace RoadRegistry.Tests.AggregateTests.GradeJunction.ModifyGradeJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void GivenOneRoadSegment_ThenGradeJunctionModified()
    {
        // Arrange
        Fixture.Freeze<GradeJunctionId>();

        var junction = GradeJunction.Create(Fixture.Create<GradeJunctionWasAdded>())
            .WithoutChanges();
        var newRoadSegmentId1 = Fixture.Create<RoadSegmentId>();

        // Act
        var problems = junction.Modify(newRoadSegmentId1, null, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        var modified = (GradeJunctionWasModified)junction.GetChanges().Single();
        modified.GradeJunctionId.Should().Be(junction.GradeJunctionId);
        modified.RoadSegmentId1.Should().Be(newRoadSegmentId1);
        modified.RoadSegmentId2.Should().BeNull();
    }

    [Fact]
    public void GivenNeitherRoadSegment_ThenNoRoadSegmentSpecifiedProblem()
    {
        // Arrange
        Fixture.Freeze<GradeJunctionId>();

        var junction = GradeJunction.Create(Fixture.Create<GradeJunctionWasAdded>())
            .WithoutChanges();

        // Act
        var problems = junction.Modify(null, null, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "GradeJunctionNoRoadSegmentSpecified");
        junction.GetChanges().Should().BeEmpty();
    }
}
