namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentMigrated()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Geometry = Fixture.Create<RoadSegmentGeometry>().ToGeometry(),
            EuropeanRoadNumbers = Fixture.CreateMany<EuropeanRoadNumber>(3).Distinct().ToList(),
            NationalRoadNumbers = Fixture.CreateMany<NationalRoadNumber>(3).Distinct().ToList()
        };

        // Act
        var (segment, problems) = RoadSegment.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentMigrated = (RoadSegmentWasMigrated)segment.GetChanges().Single();
        segmentMigrated.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentMigrated.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentMigrated.OriginalId.Should().Be(change.OriginalId);
        segmentMigrated.StartNodeId.Should().Be(change.StartNodeId);
        segmentMigrated.EndNodeId.Should().Be(change.EndNodeId);
        segmentMigrated.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentMigrated.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentMigrated.Category.Should().Be(change.Category);
        segmentMigrated.Morphology.Should().Be(change.Morphology);
        segmentMigrated.Status.Should().Be(change.Status);
        segmentMigrated.StreetNameId.Should().Be(change.StreetNameId);
        segmentMigrated.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentMigrated.SurfaceType.Should().Be(change.SurfaceType);
        segmentMigrated.EuropeanRoadNumbers.Should().BeEquivalentTo(change.EuropeanRoadNumbers);
        segmentMigrated.NationalRoadNumbers.Should().BeEquivalentTo(change.NationalRoadNumbers);
    }

    [Fact]
    public void WithOutlined_ThenRoadSegmentMigrated()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined,
            Geometry = Fixture.Create<RoadSegmentGeometry>().ToGeometry(),
            EuropeanRoadNumbers = Fixture.CreateMany<EuropeanRoadNumber>(3).Distinct().ToList(),
            NationalRoadNumbers = Fixture.CreateMany<NationalRoadNumber>(3).Distinct().ToList()
        };

        // Act
        var (segment, problems) = RoadSegment.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentMigrated = (RoadSegmentWasMigrated)segment.GetChanges().Single();
        segmentMigrated.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentMigrated.Geometry.Should().Be(change.Geometry.ToGeometryObject());
        segmentMigrated.OriginalId.Should().Be(change.OriginalId);
        segmentMigrated.StartNodeId.Should().Be(change.StartNodeId);
        segmentMigrated.EndNodeId.Should().Be(change.EndNodeId);
        segmentMigrated.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentMigrated.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentMigrated.Category.Should().Be(change.Category);
        segmentMigrated.Morphology.Should().Be(change.Morphology);
        segmentMigrated.Status.Should().Be(change.Status);
        segmentMigrated.StreetNameId.Should().Be(change.StreetNameId);
        segmentMigrated.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentMigrated.SurfaceType.Should().Be(change.SurfaceType);
        segmentMigrated.EuropeanRoadNumbers.Should().BeEquivalentTo(change.EuropeanRoadNumbers);
        segmentMigrated.NationalRoadNumbers.Should().BeEquivalentTo(change.NationalRoadNumbers);
    }

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString()
        };

        // Act
        var (_, problems) = RoadSegment.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().ContainEquivalentOf(new RoadSegmentGeometryLengthIsZero(change.OriginalId!.Value));
    }

    [Fact]
    public void EnsureAttributesValidatorIsUsed()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(null, RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategory>())
                .Add(null, RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategory>())
        };

        // Act
        var (_, problems) = RoadSegment.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment");
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var evt = Fixture.Create<RoadSegmentWasMigrated>();

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
