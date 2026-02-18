namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
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
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            Geometry = BuildRoadSegmentGeometry(TestData.StartPoint1, TestData.EndPoint1),
            EuropeanRoadNumbers = Fixture.CreateMany<EuropeanRoadNumber>(3).Distinct().ToList(),
            NationalRoadNumbers = Fixture.CreateMany<NationalRoadNumber>(3).Distinct().ToList()
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ], [], []);
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var (segment, problems) = RoadSegment.Migrate(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentMigrated = (RoadSegmentWasMigrated)segment.GetChanges().Single();
        segmentMigrated.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentMigrated.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentMigrated.OriginalId.Should().Be(change.OriginalId);
        segmentMigrated.StartNodeId.Should().Be(TestData.Segment1StartNodeAdded.RoadNodeId);
        segmentMigrated.EndNodeId.Should().Be(TestData.Segment1EndNodeAdded.RoadNodeId);
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
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Geometry = Fixture.Create<RoadSegmentGeometry>(),
            EuropeanRoadNumbers = Fixture.CreateMany<EuropeanRoadNumber>(3).Distinct().ToList(),
            NationalRoadNumbers = Fixture.CreateMany<NationalRoadNumber>(3).Distinct().ToList()
        };

        var roadNetwork = new RoadNetworkBuilder(new FakeRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var (segment, problems) = RoadSegment.Migrate(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentMigrated = (RoadSegmentWasMigrated)segment.GetChanges().Single();
        segmentMigrated.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentMigrated.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentMigrated.OriginalId.Should().Be(change.OriginalId);
        segmentMigrated.StartNodeId.Should().Be(new RoadNodeId(0));
        segmentMigrated.EndNodeId.Should().Be(new RoadNodeId(0));
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
            Geometry = RoadSegmentGeometry.Create(new LineString([new Coordinate(0, 0), new Coordinate(0.0001, 0)]).ToMultiLineString())
        };

        // Act
        var (_, problems) = RoadSegment.Migrate(change, Fixture.Create<ScopedRoadNetworkContext>());

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
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(1, 0)]).ToMultiLineString().ToRoadSegmentGeometry(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
        };

        // Act
        var (_, problems) = RoadSegment.Migrate(change, Fixture.Create<ScopedRoadNetworkContext>());

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
        segment.Geometry.Should().BeEquivalentTo(evt.Geometry);
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
