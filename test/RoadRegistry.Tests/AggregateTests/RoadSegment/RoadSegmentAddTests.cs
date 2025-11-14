namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

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

    [Fact]
    public void WithOutlined_WhenGeometryLengthIsLessThanMinimum_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(1.99, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsLessThanMinimum(change.OriginalId!.Value, Distances.TooClose));
    }

    [Fact]
    public void WithOutlined_WhenGeometryLengthIsTooLong_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(100000.01, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsTooLong(change.OriginalId!.Value, Distances.TooLongSegmentLength));
    }

    [Fact]
    public void WhenGeometryLengthIsZero_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsZero(change.OriginalId!.Value));
    }

    [Fact]
    public void WhenGeometryLengthIsTooLong_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(100000.01, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsTooLong(change.OriginalId!.Value, Distances.TooLongSegmentLength));
    }

    [Fact]
    public void WhenGeometrySelfOverlaps_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0, 0), new Coordinate(5, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometrySelfOverlaps(change.OriginalId!.Value));
    }

    [Fact]
    public void WhenGeometrySelfIntersects_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 1), new Coordinate(0, -1)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometrySelfIntersects(change.OriginalId!.Value));
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_AccessRestriction()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<RoadSegmentAccessRestriction>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Category()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<RoadSegmentCategory>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Morphology()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<RoadSegmentMorphology>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Status()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<RoadSegmentStatus>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_StreetNameId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<StreetNameLocalId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_MaintenanceAuthorityId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<OrganizationId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_SurfaceType()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>()
                .Add(null, Fixture.Create<RoadSegmentPosition>(), Fixture.Create<RoadSegmentAttributeSide>(), Fixture.Create<RoadSegmentSurfaceType>())
        });
    }

    [Fact]
    public void WhenEuropeanRoadsAreNotUnique_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            EuropeanRoadNumbers = [EuropeanRoadNumber.E17, EuropeanRoadNumber.E17]
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().Contain(x => x.Reason == "RoadSegmentEuropeanRoadsNotUnique");
    }

    [Fact]
    public void WhenNationalRoadsAreNotUnique_ThenError()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates(),
            NationalRoadNumbers = [NationalRoadNumber.Parse("N001"), NationalRoadNumber.Parse("N001")]
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().Contain(x => x.Reason == "RoadSegmentNationalRoadsNotUnique");
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
    }

    private void EnsureValidatorIsUsedForAttribute(Func<AddRoadSegmentChange, AddRoadSegmentChange> changeBuilder)
    {
        // Arrange
        var change = changeBuilder(Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates()
        });

        // Act
        var (_, problems) = RoadSegment.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeTrue();
        problems.Should().Contain(x => x.Reason.EndsWith("FromOrToPositionIsNull"));
    }
}
