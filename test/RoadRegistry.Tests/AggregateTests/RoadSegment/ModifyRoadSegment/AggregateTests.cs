namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegment;

using AutoFixture;
using Extensions;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates()
        };

        // Act
        var problems = segment.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentWasModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentModified.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentModified.OriginalId.Should().Be(change.OriginalId);
        segmentModified.StartNodeId.Should().Be(change.StartNodeId!.Value);
        segmentModified.EndNodeId.Should().Be(change.EndNodeId!.Value);
        segmentModified.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.Status.Should().Be(change.Status);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
    }

    [Fact]
    public void WithOutlined_ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates()
        };

        // Act
        var problems = segment.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentWasModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentModified.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentModified.OriginalId.Should().Be(change.OriginalId);
        segmentModified.StartNodeId.Should().Be(change.StartNodeId!.Value);
        segmentModified.EndNodeId.Should().Be(change.EndNodeId!.Value);
        segmentModified.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.Status.Should().Be(change.Status);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
    }

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString()
        };

        // Act
        var problems = segment.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsZero(change.OriginalId!.Value));
    }

    [Fact]
    public void EnsureAttributesValidatorIsUsed()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(null, RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategory>())
                .Add(null, RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategory>())
        };

        // Act
        var problems = segment.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment");
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>();
        var segment = RoadSegment.Create(segmentAdded);
        var evt = Fixture.Create<RoadSegmentWasModified>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.AsText().Should().Be(evt.Geometry!.WKT);
        segment.StartNodeId.Should().Be(evt.StartNodeId!.Value);
        segment.EndNodeId.Should().Be(evt.EndNodeId!.Value);
        segment.Attributes.GeometryDrawMethod.Should().Be(evt.GeometryDrawMethod);
        segment.Attributes.AccessRestriction.Should().Be(evt.AccessRestriction);
        segment.Attributes.Category.Should().Be(evt.Category);
        segment.Attributes.Morphology.Should().Be(evt.Morphology);
        segment.Attributes.Status.Should().Be(evt.Status);
        segment.Attributes.StreetNameId.Should().Be(evt.StreetNameId);
        segment.Attributes.MaintenanceAuthorityId.Should().Be(evt.MaintenanceAuthorityId);
        segment.Attributes.SurfaceType.Should().Be(evt.SurfaceType);
    }
}
