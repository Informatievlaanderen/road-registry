namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegment;

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
    public void WithStatusGerealiseerd_ThenRoadSegmentModified()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>() with { Status = RoadSegmentStatusV2.Gerealiseerd };
        var segment = RoadSegment.Create(segmentAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            GeometryDrawMethod = segmentAdded.GeometryDrawMethod,
            Geometry = BuildRoadSegmentGeometry(TestData.StartPoint1, TestData.EndPoint1)
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ], [], []);
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentModified = (RoadSegmentWasChanged)segment.GetChanges().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentIdReference.RoadSegmentId);
        segmentModified.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentModified.OriginalRoadSegmentIdReference.Should().Be(change.RoadSegmentIdReference);
        segmentModified.NodeIds.Should().NotBeNull();
        segmentModified.NodeIds!.Start.Should().Be(TestData.Segment1StartNodeAdded.RoadNodeId);
        segmentModified.NodeIds.End.Should().Be(TestData.Segment1EndNodeAdded.RoadNodeId);
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
    }

    [Theory]
    [InlineData("Gepland")]
    [InlineData("NietGerealiseerd")]
    [InlineData("BuitenGebruik")]
    [InlineData("Gehistoreerd")]
    public void WithStatusNotGerealiseerd_ThenTheStatusPathIsFollowedAndTheRestIsChanged(string status)
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        // Start from a realized segment so the status change detaches it from its road nodes.
        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>() with { Status = RoadSegmentStatusV2.Gerealiseerd };
        var segment = RoadSegment.Create(segmentAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            GeometryDrawMethod = segmentAdded.GeometryDrawMethod,
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry()
        };

        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();

        var expectedPath = RoadSegmentStatusPaths.Get(RoadSegmentStatusV2.Gerealiseerd, change.Status!);
        segment.GetChanges().Select(x => x.GetType())
            .Should().Equal(expectedPath.Select(x => x.EventType).Append(typeof(RoadSegmentWasChanged)));
        segment.Status.Should().Be(change.Status);
        segment.StartNodeId.Should().BeNull();
        segment.EndNodeId.Should().BeNull();

        var segmentModified = segment.GetChanges().OfType<RoadSegmentWasChanged>().Single();
        segmentModified.RoadSegmentId.Should().Be(change.RoadSegmentIdReference.RoadSegmentId);
        segmentModified.Geometry.Should().BeEquivalentTo(change.Geometry);
        segmentModified.OriginalRoadSegmentIdReference.Should().Be(change.RoadSegmentIdReference);
        segmentModified.NodeIds.Should().BeNull();
        segmentModified.AccessRestriction.Should().Be(change.AccessRestriction);
        segmentModified.Category.Should().Be(change.Category);
        segmentModified.Morphology.Should().Be(change.Morphology);
        segmentModified.StreetNameId.Should().Be(change.StreetNameId);
        segmentModified.MaintenanceAuthorityId.Should().Be(change.MaintenanceAuthorityId);
        segmentModified.SurfaceType.Should().Be(change.SurfaceType);
    }

    [Theory]
    [InlineData("Gepland")]
    [InlineData("NietGerealiseerd")]
    [InlineData("BuitenGebruik")]
    [InlineData("Gehistoreerd")]
    public void WhenBecomingGerealiseerd_ThenTheLastStatusChangeCarriesEverythingAndNothingFollows(string status)
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            StartNodeId = null,
            EndNodeId = null
        };
        var segment = RoadSegment.Create(segmentAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gerealiseerd,
            GeometryDrawMethod = segmentAdded.GeometryDrawMethod,
            Geometry = BuildRoadSegmentGeometry(TestData.StartPoint1, TestData.EndPoint1)
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ], [], []);
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();

        var expectedPath = RoadSegmentStatusPaths.Get(RoadSegmentStatusV2.Parse(status), RoadSegmentStatusV2.Gerealiseerd);
        segment.GetChanges().Select(x => x.GetType()).Should().Equal(expectedPath.Select(x => x.EventType));

        var connected = segment.GetChanges().OfType<IRoadSegmentWasConnectedEvent>().Single();
        connected.Geometry.Should().BeEquivalentTo(change.Geometry);
        connected.StartNodeId.Should().Be(TestData.Segment1StartNodeAdded.RoadNodeId);
        connected.EndNodeId.Should().Be(TestData.Segment1EndNodeAdded.RoadNodeId);
        connected.Category.Should().Be(change.Category);
        segment.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
    }

    [Theory]
    [InlineData("Ingeschetst", "Ingemeten", typeof(RoadSegmentWasMeasured))]
    [InlineData("Ingemeten", "Ingeschetst", typeof(RoadSegmentWasCorrectedToOutlined))]
    public void WhenGeometryDrawMethodChanges_ThenItIsRecordedOnItsOwnBeforeTheRest(string from, string to, Type expectedEvent)
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>() with
        {
            Status = RoadSegmentStatusV2.Gepland,
            StartNodeId = null,
            EndNodeId = null,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Parse(from)
        };
        var segment = RoadSegment.Create(segmentAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gepland,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Parse(to),
            Geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry()
        };

        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Select(x => x.GetType()).Should().Equal(expectedEvent, typeof(RoadSegmentWasChanged));
        segment.Attributes!.GeometryDrawMethod.Should().Be(change.GeometryDrawMethod);
    }

    [Fact]
    public void WhenOnlyTheGeometryDrawMethodChanges_ThenNoRoadSegmentWasChanged()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>() with
        {
            Status = RoadSegmentStatusV2.Gepland,
            StartNodeId = null,
            EndNodeId = null,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
        };
        var segment = RoadSegment.Create(segmentAdded)
            .WithoutChanges();
        var change = new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(segment.RoadSegmentId),
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten
        };

        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentWasMeasured>();
    }

    [Fact]
    public void WhenNoChangeDetected_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(segment.RoadSegmentId),
            Status = segment.Status
        };

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [
            RoadNode.Create(TestData.Segment1StartNodeAdded),
            RoadNode.Create(TestData.Segment1EndNodeAdded)
        ], [], []);
        var roadNetworkContext = new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);

        // Act
        var problems = segment.Modify(change, roadNetworkContext);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().BeEmpty();
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
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Beide, RoadSegmentCategoryV2.EuropeseHoofdweg)
                .Add(new RoadSegmentPositionCoverage(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Beide, RoadSegmentCategoryV2.InterlokaleWeg)
        };

        // Act
        var problems = segment.Modify(change, Fixture.Create<ScopedRoadNetworkChangeContext>());

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
        var evt = Fixture.Create<RoadSegmentWasChanged>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.Should().BeEquivalentTo(evt.Geometry);
        segment.StartNodeId.Should().Be(evt.NodeIds!.Start!.Value);
        segment.EndNodeId.Should().Be(evt.NodeIds.End!.Value);
        segment.Attributes!.AccessRestriction.Should().Be(evt.AccessRestriction);
        segment.Attributes!.Category.Should().Be(evt.Category);
        segment.Attributes!.Morphology.Should().Be(evt.Morphology);
        segment.Attributes!.StreetNameId.Should().Be(evt.StreetNameId);
        segment.Attributes!.MaintenanceAuthorityId.Should().Be(evt.MaintenanceAuthorityId);
        segment.Attributes!.SurfaceType.Should().Be(evt.SurfaceType);
    }
}
