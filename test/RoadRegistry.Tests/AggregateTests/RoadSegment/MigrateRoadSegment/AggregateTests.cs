namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithStatusGerealiseerd_ThenRoadSegmentMigrated()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
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
        var problems = segment.Migrate(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentMigrated = (RoadSegmentWasMigrated)segment.GetChanges().Single();
        segmentMigrated.RoadSegmentId.Should().Be(change.RoadSegmentIdReference.RoadSegmentId);
        segmentMigrated.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentMigrated.OriginalRoadSegmentIdReference.Should().BeEquivalentTo(change.RoadSegmentIdReference);
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

    [Theory]
    [InlineData("Gepland")]
    [InlineData("NietGerealiseerd")]
    [InlineData("BuitenGebruik")]
    [InlineData("Gehistoreerd")]
    public void WithStatusNotGerealiseerd_ThenRoadSegmentMigrated(string status)
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            Geometry = Fixture.Create<RoadSegmentGeometry>(),
            EuropeanRoadNumbers = Fixture.CreateMany<EuropeanRoadNumber>(3).Distinct().ToList(),
            NationalRoadNumbers = Fixture.CreateMany<NationalRoadNumber>(3).Distinct().ToList()
        };

        var roadNetwork = new RoadNetworkBuilder(new FakeRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var problems = segment.Migrate(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentMigrated = (RoadSegmentWasMigrated)segment.GetChanges().Single();
        segmentMigrated.RoadSegmentId.Should().Be(change.RoadSegmentIdReference.RoadSegmentId);
        segmentMigrated.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentMigrated.OriginalRoadSegmentIdReference.Should().BeEquivalentTo(change.RoadSegmentIdReference);
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

    [Theory]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        false
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        true
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601004, 601005.01 601004, 601005.01 601006, 601010 601006))",
        false
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601004, 601005.01 601004, 601005.01 601006, 601010 601006))",
        true
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005 601000, 601000 601005, 601005 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        false
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005 601000, 601000 601005, 601005 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))",
        true
    )]
    public void GivenPartiallyOverlappingSegments_ThenError(string segment1Geometry, string segment2Geometry, bool swapGeometry)
    {
        // Arrange
        var existingGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(swapGeometry ? segment1Geometry : segment2Geometry).WithSrid(WellknownSrids.Lambert08));
        var newGeometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(swapGeometry ? segment2Geometry : segment1Geometry).WithSrid(WellknownSrids.Lambert08));

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [
            RoadSegment.Create(TestData.Segment1Added with
            {
                Status = RoadSegmentStatusV2.Gerealiseerd,
                Geometry = existingGeometry
            })
        ], []);
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        Fixture.Freeze(new RoadSegmentId(2));
        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Geometry = newGeometry,
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        // Act
        var problems = segment.Migrate(change, roadNetworkContext);

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
    }

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Geometry = RoadSegmentGeometry.Create(new LineString([new Coordinate(0, 0), new Coordinate(0.9, 0)]).ToMultiLineString())
        };

        var roadNetworkContext = new ScopedRoadNetworkContext(
            new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
                RoadNode.Create(Fixture.Create<RoadNodeWasAdded>() with
                {
                    Geometry = RoadNodeGeometry.Create(change.Geometry.Value.GetSingleLineString().StartPoint)
                }),
                RoadNode.Create(Fixture.Create<RoadNodeWasAdded>() with
                {
                    Geometry = RoadNodeGeometry.Create(change.Geometry.Value.GetSingleLineString().EndPoint)
                })
            ], [], []),
            new IdentifierTranslator(),
            TestData.Provenance);

        // Act
        var problems = segment.Migrate(change, roadNetworkContext);

        // Assert
        problems.Should().ContainEquivalentOf(
            new Error("RoadSegmentGeometryLengthLessThanMinimum",
                new ProblemParameter("Minimum", 1.ToString()),
                new ProblemParameter("WegsegmentId", change.RoadSegmentIdReference.RoadSegmentId.ToString()),
                new ProblemParameter("WegsegmentTempIds", change.RoadSegmentIdReference.GetTempIdsAsString())
            )
        );
    }

    [Fact]
    public void EnsureAttributesValidatorIsUsed()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(1, 0)]).ToMultiLineString().ToRoadSegmentGeometry(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
        };

        // Act
        var problems = segment.Migrate(change, Fixture.Create<ScopedRoadNetworkContext>());

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
        var evt = Fixture.Create<RoadSegmentWasMigrated>();

        // Act
        segment.Apply(evt);

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
