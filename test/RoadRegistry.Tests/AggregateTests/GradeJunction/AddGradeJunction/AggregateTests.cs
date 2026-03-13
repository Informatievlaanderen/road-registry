namespace RoadRegistry.Tests.AggregateTests.GradeJunction.AddGradeJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Changes;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeJunctionAdded()
    {
        // Arrange
        var change = Fixture.Create<AddGradeJunctionChange>();

        // Act
        var (junction, problems) = GradeJunction.Add(change, TestData.Provenance, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.Should().HaveNoError();
        junction.GetChanges().Should().HaveCount(1);

        var junctionAdded = (GradeJunctionWasAdded)junction.GetChanges().Single();
        junctionAdded.GradeJunctionId.Should().Be(new GradeJunctionId(1));
        junctionAdded.Type.Should().Be(change.Type);
        junctionAdded.LowerRoadSegmentId.Should().Be(change.LowerRoadSegmentId);
        junctionAdded.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<GradeJunctionWasAdded>();

        // Act
        var junction = GradeJunction.Create(evt);

        // Assert
        junction.GradeJunctionId.Should().Be(evt.GradeJunctionId);
        junction.Type.Should().Be(evt.Type);
        junction.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId);
        junction.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId);
    }
}
