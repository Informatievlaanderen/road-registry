namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MergeRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentMerged()
    {
        // Arrange
        var change = Fixture.Create<MergeRoadSegmentChange>() with
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
        var (segment, problems) = RoadSegment.Merge(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), idTranslator);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentWasMerged)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentAdded.OriginalIds.Should().BeEquivalentTo(change.OriginalIds);
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
    public void WithOutlined_ThenRoadSegmentMerged()
    {
        // Arrange
        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        // Act
        var (segment, problems) = RoadSegment.Merge(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentWasMerged)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentAdded.OriginalIds.Should().BeEquivalentTo(change.OriginalIds);
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
        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Merge(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsZero(change.TemporaryId));
    }

    [Fact]
    public void EnsureAttributesValidatorIsUsed()
    {
        // Arrange
        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(null, RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategory>())
                .Add(null, RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategory>())
        };

        // Act
        var (_, problems) = RoadSegment.Merge(change, TestData.Provenance, new FakeRoadNetworkIdGenerator(), new IdentifierTranslator());

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment");
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadSegmentWasMerged>();

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
