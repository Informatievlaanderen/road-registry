namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.RemoveGradeSeparatedJunctionBecauseOfMigration;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionRemoved()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var junction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionWasAdded>())
            .WithoutChanges();

        // Act
        var problems = junction.RemoveBecauseOfMigration(Fixture.Create<Provenance>());

        // Assert
        problems.Should().HaveNoError();
        junction.GetChanges().Should().HaveCount(1);

        var junctionRemoved = (GradeSeparatedJunctionWasRemovedBecauseOfMigration)junction.GetChanges().Single();
        junctionRemoved.GradeSeparatedJunctionId.Should().Be(junction.GradeSeparatedJunctionId);
        junctionRemoved.LowerRoadSegmentId.Should().Be(junction.LowerRoadSegmentId);
        junctionRemoved.UpperRoadSegmentId.Should().Be(junction.UpperRoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var junctionAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var junction = GradeSeparatedJunction.Create(junctionAdded);

        var evt = Fixture.Create<GradeSeparatedJunctionWasRemovedBecauseOfMigration>();

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
