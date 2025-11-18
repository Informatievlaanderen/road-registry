namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadSegmentModifyTests : RoadNetworkTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        // Act
        var problems = segment.Modify(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentModified.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentModified.OriginalId.Should().Be(change.OriginalId ?? change.RoadSegmentId);
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
        segmentModified.EuropeanRoadNumbers.Should().BeEquivalentTo(change.EuropeanRoadNumbers);
        segmentModified.NationalRoadNumbers.Should().BeEquivalentTo(change.NationalRoadNumbers);
    }

    [Fact]
    public void WithOutlined_ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        // Act
        var problems = segment.Modify(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentModified.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentModified.OriginalId.Should().Be(change.OriginalId ?? change.RoadSegmentId);
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
        segmentModified.EuropeanRoadNumbers.Should().BeEquivalentTo(change.EuropeanRoadNumbers);
        segmentModified.NationalRoadNumbers.Should().BeEquivalentTo(change.NationalRoadNumbers);
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = new ModifyRoadSegmentChange
        {
            RoadSegmentId = TestData.Segment1Added.RoadSegmentId
        };

        return Run(scenario => ScenarioExtensions.ThenProblems(scenario
                .Given(changes => changes)
                .When(changes => changes.Add(change)), new Error("RoadSegmentNotFound", [new("SegmentId", change.RoadSegmentId.ToString())]))
        );
    }

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsZero(change.OriginalId!.Value));
    }

    [Fact]
    public void EnsureAttributesValidatorIsUsed()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<RoadSegmentCategory>())
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryFromOrToPositionIsNull");
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>());
        var evt = Fixture.Create<RoadSegmentModified>();

        // Act
        segment.Apply(evt);

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
