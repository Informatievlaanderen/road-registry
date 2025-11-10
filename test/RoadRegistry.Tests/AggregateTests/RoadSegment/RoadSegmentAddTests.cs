namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAddTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadSegmentAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            PermanentId = null,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates()
        };

        // Act
        var (segment, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentAdded)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentAdded.OriginalId.Should().Be(change.OriginalId ?? change.TemporaryId);
        segmentAdded.StartNodeId.Should().Be(change.StartNodeId);
        segmentAdded.EndNodeId.Should().Be(change.EndNodeId);
        segmentAdded.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentAdded.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentAdded.Category.Should().Be(change.Category);
        segmentAdded.Morphology.Should().Be(change.Morphology);
        segmentAdded.Status.Should().Be(change.Status);
        segmentAdded.StreetNameId.Should().Be(change.StreetNameId);
        segmentAdded.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentAdded.SurfaceType.Should().Be(change.SurfaceType);
        segmentAdded.EuropeanRoadNumbers.Should().BeEquivalentTo(change.EuropeanRoadNumbers);
        segmentAdded.NationalRoadNumbers.Should().BeEquivalentTo(change.NationalRoadNumbers);
    }

    //TODO-pr test validations RoadSegment.Add
    // [Fact]
    // public Task WhenStartNodeIsMissing_ThenError()
    // {
    //     var change = new ModifyRoadSegmentChange
    //     {
    //         RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
    //         StartNodeId = new RoadNodeId(9)
    //     };
    //
    //     return Run(scenario => scenario
    //         .Given(changes => changes
    //             .Add(TestData.AddStartNode1)
    //             .Add(TestData.AddEndNode1)
    //             .Add(TestData.AddSegment1)
    //         )
    //         .When(changes => changes.Add(change))
    //         .Throws(new Error("RoadSegmentStartNodeMissing", [new("Identifier", "1")]))
    //     );
    // }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadSegmentAdded>();

        // Act
        var segment = RoadSegment.Create(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.AsText().Should().Be(evt.Geometry.WKT);
        segment.StartNodeId.Should().Be(evt.StartNodeId);
        segment.EndNodeId.Should().Be(evt.EndNodeId);
        segment.Attributes.GeometryDrawMethod.Should().Be(evt.GeometryDrawMethod);
        segment.Attributes.AccessRestriction.Should().Be(evt.AccessRestriction);
        segment.Attributes.Category.Should().Be(evt.Category);
        segment.Attributes.Morphology.Should().Be(evt.Morphology);
        segment.Attributes.Status.Should().Be(evt.Status);
        segment.Attributes.StreetNameId.Should().Be(evt.StreetNameId);
        segment.Attributes.MaintenanceAuthorityId.Should().Be(evt.MaintenanceAuthorityId);
        segment.Attributes.SurfaceType.Should().Be(evt.SurfaceType);
        segment.Attributes.EuropeanRoadNumbers.Should().BeEquivalentTo(evt.EuropeanRoadNumbers);
        segment.Attributes.NationalRoadNumbers.Should().BeEquivalentTo(evt.NationalRoadNumbers);
    }
}
