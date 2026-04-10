namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
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
    public void WithStatusGerealiseerd_ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Geometry = BuildRoadSegmentGeometry(TestData.StartPoint1, TestData.EndPoint1)
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ], [], []);
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentWasModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentIdReference.RoadSegmentId);
        segmentModified.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentModified.OriginalRoadSegmentIdReference.Should().Be(change.RoadSegmentIdReference);
        segmentModified.StartNodeId.Should().Be(TestData.Segment1StartNodeAdded.RoadNodeId);
        segmentModified.EndNodeId.Should().Be(TestData.Segment1EndNodeAdded.RoadNodeId);
        segmentModified.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.Status.Should().Be(change.Status);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
    }

    [Theory]
    [InlineData("Gepland")]
    [InlineData("NietGerealiseerd")]
    [InlineData("BuitenGebruik")]
    [InlineData("Gehistoreerd")]
    public void WithStatusNotGerealiseerd_ThenRoadSegmentModified(string status)
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry()
        };

        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentWasModified)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentIdReference.RoadSegmentId);;
        segmentModified.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentModified.OriginalRoadSegmentIdReference.Should().Be(change.RoadSegmentIdReference);
        segmentModified.StartNodeId.Should().BeNull();
        segmentModified.EndNodeId.Should().BeNull();
        segmentModified.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.Status.Should().Be(change.Status);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
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
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Geometry = newGeometry
        };

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

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
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
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
        var problems = segment.Modify(change, roadNetworkContext);

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
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(1, 0)]).ToMultiLineString().ToRoadSegmentGeometry(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, Fixture.Create<RoadSegmentCategoryV2>())
        };

        // Act
        var problems = segment.Modify(change, Fixture.Create<ScopedRoadNetworkContext>());

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
        segment.Geometry.Should().BeEquivalentTo(evt.Geometry);
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
