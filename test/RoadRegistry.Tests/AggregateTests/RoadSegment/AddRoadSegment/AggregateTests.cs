namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.ValueObjects.Problems;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithStatusGerealiseerd_ThenRoadSegmentAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Geometry = BuildRoadSegmentGeometry(TestData.StartPoint1, TestData.EndPoint1),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ]);
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var (segment, problems) = RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentWasAdded)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentAdded.OriginalRoadSegmentIdReference.Should().BeEquivalentTo(change.RoadSegmentIdReference);
        segmentAdded.StartNodeId.Should().Be(TestData.Segment1StartNodeAdded.RoadNodeId);
        segmentAdded.EndNodeId.Should().Be(TestData.Segment1EndNodeAdded.RoadNodeId);
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

    [Theory]
    [InlineData("Gepland")]
    [InlineData("NietGerealiseerd")]
    [InlineData("BuitenGebruik")]
    [InlineData("Gehistoreerd")]
    public void WithStatusNotGerealiseerd_ThenRoadSegmentAdded(string status)
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var (segment, problems) = RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentWasAdded)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(new RoadSegmentId(1));
        segmentAdded.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentAdded.OriginalRoadSegmentIdReference.Should().BeEquivalentTo(change.RoadSegmentIdReference);
        segmentAdded.StartNodeId.Should().BeNull();
        segmentAdded.EndNodeId.Should().BeNull();
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
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Geometry = newGeometry,
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), roadNetworkContext);

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
    }

    [Theory]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601010, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))"
    )]
    [InlineData(
        "MULTILINESTRING ((601000 601000, 601005.00 601005, 601000 601010))",
        "MULTILINESTRING ((601010 601000, 601005.01 601000, 601005.01 601010, 601010 601010))"
    )]
    public void GivenNoPartiallyOverlappingSegments_ThenResult(string segment1Geometry, string segment2Geometry)
    {
        var reversedValues = new[] { false, true };
        foreach (var reversedValue in reversedValues)
        {
            // Arrange
            var change = Fixture.Create<AddRoadSegmentChange>() with
            {
                Status = RoadSegmentStatusV2.Gerealiseerd,
                Geometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(reversedValue ? segment2Geometry : segment1Geometry).WithSrid(WellknownSrids.Lambert08)),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = []
            };

            var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [
                RoadSegment.Create(TestData.Segment2Added with
                {
                    Status = RoadSegmentStatusV2.Gerealiseerd,
                    Geometry = RoadSegmentGeometry.Create((MultiLineString)new WKTReader().Read(reversedValue ? segment1Geometry : segment2Geometry).WithSrid(WellknownSrids.Lambert08))
                })
            ], []);
            var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

            // Act
            var (_, problems) = RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), roadNetworkContext);

            // Assert
            problems.Should().NotContain(x => x.Reason == "RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
        }
    }

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = RoadSegmentGeometry.Create(new LineString([new Coordinate(0, 0), new Coordinate(0.9, 0)]).ToMultiLineString())
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), Fixture.Create<ScopedRoadNetworkContext>());

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
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(1, 0)]).ToMultiLineString().ToRoadSegmentGeometry(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
        };

        // Act
        var (_, problems) = RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), Fixture.Create<ScopedRoadNetworkContext>());

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment");
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadSegmentWasAdded>();

        // Act
        var segment = RoadSegment.Create(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.Should().Be(evt.Geometry);
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
