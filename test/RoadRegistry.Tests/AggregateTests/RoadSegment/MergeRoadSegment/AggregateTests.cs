namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MergeRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
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
using RoadRegistry.ValueObjects.Problems;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithStatusGerealiseerd_ThenRoadSegmentMerged()
    {
        // Arrange
        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();

        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            RoadSegmentId = segment.RoadSegmentId,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            Geometry = BuildRoadSegmentGeometry(TestData.StartPoint1, TestData.EndPoint1),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ], [], []);
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var problems = segment.Merge(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentWasMerged)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(segment.RoadSegmentId);
        segmentAdded.OtherRoadSegmentId.Should().BeEquivalentTo(change.OtherRoadSegmentId);
        segmentAdded.Geometry.Should().BeEquivalentTo(change.Geometry);
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
    public void WithStatusNotGerealiseerd_ThenRoadSegmentMerged(string status)
    {
        // Arrange
        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();

        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            RoadSegmentId = segment.RoadSegmentId,
            Status = RoadSegmentStatusV2.Parse(status),
            Geometry = Fixture.Create<MultiLineString>().ToRoadSegmentGeometry(),
            EuropeanRoadNumbers = [Fixture.Create<EuropeanRoadNumber>()],
            NationalRoadNumbers = [Fixture.Create<NationalRoadNumber>()]
        };

        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, new IdentifierTranslator(), TestData.Provenance);

        // Act
        var problems = segment.Merge(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentAdded = (RoadSegmentWasMerged)segment.GetChanges().Single();
        segmentAdded.RoadSegmentId.Should().Be(change.RoadSegmentId);
        segmentAdded.OtherRoadSegmentId.Should().BeEquivalentTo(change.OtherRoadSegmentId);
        segmentAdded.Geometry.Should().BeEquivalentTo(change.Geometry);
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

    [Fact]
    public void EnsureGeometryValidatorIsUsed()
    {
        // Arrange
        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();

        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            RoadSegmentId = segment.RoadSegmentId,
            Geometry = RoadSegmentGeometry.Create(new LineString([new Coordinate(0, 0), new Coordinate(0.9, 0)]).ToMultiLineString())
        };

        var roadNetworkContext = new ScopedRoadNetworkChangeContext(
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
        var problems = segment.Merge(change, roadNetworkContext);

        // Assert
        problems.Should().ContainEquivalentOf(
            new Error("RoadSegmentGeometryLengthLessThanMinimum",
                new ProblemParameter("Minimum", 1.ToString()),
                new ProblemParameter("WegsegmentId", change.RoadSegmentId.ToString())
            )
        );
    }

    [Fact]
    public void EnsureAttributesValidatorIsUsed()
    {
        // Arrange
        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();

        var change = Fixture.Create<MergeRoadSegmentChange>() with
        {
            RoadSegmentId = segment.RoadSegmentId,
            Geometry = new LineString([new Coordinate(0, 0), new Coordinate(1, 0)]).ToMultiLineString().ToRoadSegmentGeometry(),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentCategoryV2.EuropeseHoofdweg)
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentCategoryV2.InterlokaleWeg)
        };

        // Act
        var problems = segment.Merge(change, Fixture.Create<ScopedRoadNetworkChangeContext>());

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentCategoryValueNotUniqueWithinSegment");
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>();
        var segment = RoadSegment.Create(segmentAdded);
        var evt = Fixture.Create<RoadSegmentWasMerged>() with
        {
            RoadSegmentId = segment.RoadSegmentId
        };

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.Should().BeEquivalentTo(evt.Geometry);
        segment.StartNodeId.Should().Be(evt.StartNodeId);
        segment.EndNodeId.Should().Be(evt.EndNodeId);
        segment.Attributes!.GeometryDrawMethod.Should().Be(evt.GeometryDrawMethod);
        segment.Attributes!.AccessRestriction.Should().Be(evt.AccessRestriction);
        segment.Attributes!.Category.Should().Be(evt.Category);
        segment.Attributes!.Morphology.Should().Be(evt.Morphology);
        segment.Attributes!.Status.Should().Be(evt.Status);
        segment.Attributes!.StreetNameId.Should().Be(evt.StreetNameId);
        segment.Attributes!.MaintenanceAuthorityId.Should().Be(evt.MaintenanceAuthorityId);
        segment.Attributes!.SurfaceType.Should().Be(evt.SurfaceType);
        segment.Attributes!.EuropeanRoadNumbers.Should().BeEquivalentTo(evt.EuropeanRoadNumbers);
        segment.Attributes!.NationalRoadNumbers.Should().BeEquivalentTo(evt.NationalRoadNumbers);
    }
}
