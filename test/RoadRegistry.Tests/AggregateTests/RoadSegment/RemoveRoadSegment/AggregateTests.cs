namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegment;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.Events.V2;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadSegmentRemoved()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();

        // Act
        var problems = segment.Remove(Fixture.Create<Provenance>());

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentRemoved = (RoadSegmentRemoved)segment.GetChanges().Single();
        segmentRemoved.RoadSegmentId.Should().Be(segment.RoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentAdded>();
        var segment = RoadSegment.Create(segmentAdded);

        var evt = Fixture.Create<RoadSegmentRemoved>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.IsRemoved.Should().BeTrue();
        segment.Geometry.AsText().Should().Be(segmentAdded.Geometry.WKT);
        segment.StartNodeId.Should().Be(segmentAdded.StartNodeId);
        segment.EndNodeId.Should().Be(segmentAdded.EndNodeId);
        segment.Attributes.GeometryDrawMethod.Should().Be(segmentAdded.GeometryDrawMethod);
        segment.Attributes.AccessRestriction.Should().Be(segmentAdded.AccessRestriction);
        segment.Attributes.Category.Should().Be(segmentAdded.Category);
        segment.Attributes.Morphology.Should().Be(segmentAdded.Morphology);
        segment.Attributes.Status.Should().Be(segmentAdded.Status);
        segment.Attributes.StreetNameId.Should().Be(segmentAdded.StreetNameId);
        segment.Attributes.MaintenanceAuthorityId.Should().Be(segmentAdded.MaintenanceAuthorityId);
        segment.Attributes.SurfaceType.Should().Be(segmentAdded.SurfaceType);
        segment.Attributes.EuropeanRoadNumbers.Should().BeEquivalentTo(segmentAdded.EuropeanRoadNumbers);
        segment.Attributes.NationalRoadNumbers.Should().BeEquivalentTo(segmentAdded.NationalRoadNumbers);
    }
}
