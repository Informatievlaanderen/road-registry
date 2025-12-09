namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegment;

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
    public void WithMeasured_ThenRoadSegmentAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        var actualStartNodeId = Fixture.Create<RoadNodeId>();
        var idTranslator = new IdentifierTranslator();
        idTranslator.RegisterMapping(change.StartNodeId, actualStartNodeId);

        // Act
        var (segment, problems) = RoadSegment.Add(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), idTranslator);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentAdded)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentAdded.OriginalId.Should().Be(change.OriginalId);
        segmentAdded.StartNodeId.Should().Be(actualStartNodeId);
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

    [Fact]
    public void WithOutlined_ThenRoadSegmentAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        // Act
        var (segment, problems) = RoadSegment.Add(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentAdded)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentAdded.OriginalId.Should().Be(change.OriginalId);
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

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
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
        var (_, problems) = RoadSegment.Add(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryFromOrToPositionIsNull");
    }

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
        segment.Origin.Timestamp.Should().Be(evt.Provenance.Timestamp);
        segment.Origin.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
        segment.LastModified.Timestamp.Should().Be(evt.Provenance.Timestamp);
        segment.LastModified.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
    }
}
