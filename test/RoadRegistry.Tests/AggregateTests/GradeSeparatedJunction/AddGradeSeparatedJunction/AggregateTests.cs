namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.AddGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadNetwork;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionAdded()
    {
        // Arrange
        var change = Fixture.Create<AddGradeSeparatedJunctionChange>();

        var actualLowerRoadSegmentId = Fixture.Create<RoadSegmentId>();
        var idTranslator = new IdentifierTranslator();
        idTranslator.RegisterMapping(change.LowerRoadSegmentId, actualLowerRoadSegmentId);

        // Act
        var (junction, problems) = GradeSeparatedJunction.Add(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), idTranslator);

        // Assert
        problems.Should().HaveNoError();
        junction.GetChanges().Should().HaveCount(1);

        var junctionAdded = (GradeSeparatedJunctionAdded)junction.GetChanges().Single();
        junctionAdded.GradeSeparatedJunctionId.Should().Be(new GradeSeparatedJunctionId(1));
        junctionAdded.Type.Should().Be(change.Type);
        junctionAdded.LowerRoadSegmentId.Should().Be(actualLowerRoadSegmentId);
        junctionAdded.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<GradeSeparatedJunctionAdded>();

        // Act
        var junction = GradeSeparatedJunction.Create(evt);

        // Assert
        junction.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        junction.Type.Should().Be(evt.Type);
        junction.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId);
        junction.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId);
        junction.Origin.Timestamp.Should().Be(evt.Provenance.Timestamp);
        junction.Origin.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
        junction.LastModified.Timestamp.Should().Be(evt.Provenance.Timestamp);
        junction.LastModified.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
    }
}
