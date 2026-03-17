namespace RoadRegistry.Tests.AggregateTests.GradeJunction.RemoveGradeJunction;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeJunctionRemoved()
    {
        // Arrange
        Fixture.Freeze<GradeJunctionId>();

        var junction = GradeJunction.Create(Fixture.Create<GradeJunctionWasAdded>())
            .WithoutChanges();

        // Act
        var problems = junction.Remove(Fixture.Create<Provenance>());

        // Assert
        problems.Should().HaveNoError();
        junction.GetChanges().Should().HaveCount(1);

        var junctionRemoved = (GradeJunctionWasRemoved)junction.GetChanges().Single();
        junctionRemoved.GradeJunctionId.Should().Be(junction.GradeJunctionId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<GradeJunctionId>();

        var junctionAdded = Fixture.Create<GradeJunctionWasAdded>();
        var junction = GradeJunction.Create(junctionAdded);

        var evt = Fixture.Create<GradeJunctionWasRemoved>();

        // Act
        junction.Apply(evt);

        // Assert
        junction.GradeJunctionId.Should().Be(evt.GradeJunctionId);
        junction.IsRemoved.Should().BeTrue();
        junction.RoadSegmentId1.Should().Be(junctionAdded.RoadSegmentId1);
        junction.RoadSegmentId2.Should().Be(junctionAdded.RoadSegmentId2);
    }
}
