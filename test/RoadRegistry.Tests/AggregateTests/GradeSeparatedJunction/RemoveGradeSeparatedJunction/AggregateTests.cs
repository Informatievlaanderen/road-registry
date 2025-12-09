namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.RemoveGradeSeparatedJunction;

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

        var junction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionAdded>())
            .WithoutChanges();

        // Act
        var problems = junction.Remove(Fixture.Create<Provenance>());

        // Assert
        problems.Should().HaveNoError();
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
        junction.Origin.Timestamp.Should().Be(junctionAdded.Provenance.Timestamp);
        junction.Origin.OrganizationId.Should().Be(new OrganizationId(junctionAdded.Provenance.Operator));
        junction.LastModified.Timestamp.Should().Be(evt.Provenance.Timestamp);
        junction.LastModified.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
    }
}
