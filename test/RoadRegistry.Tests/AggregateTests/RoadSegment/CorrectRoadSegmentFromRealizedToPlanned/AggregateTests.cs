namespace RoadRegistry.Tests.AggregateTests.RoadSegment.CorrectRoadSegmentFromRealizedToPlanned;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    // The segment being corrected back to 'gepland', and the neighbours it hangs off.
    private const int CorrectedSegmentId = 1;
    private const int NeighbourSegmentId = 2;
    private const int SecondNeighbourSegmentId = 3;

    private RoadNodeWasAdded BuildNode(int id, double x, double y, RoadNodeTypeV2? type = null)
    {
        return new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
            Grensknoop = false,
            Type = type,
            Provenance = new ProvenanceData(TestData.Provenance)
        };
    }

    private RoadSegmentGeometry BuildGeometry(params (double X, double Y)[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates.Select(x => new Coordinate(x.X, x.Y)).ToArray())])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }

    private static RoadSegmentDynamicAttributeValues<T> Spanning<T>(RoadSegmentDynamicAttributeValues<T> template, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>().Add(template.Values.First().Value, geometry);
    }

    private RoadSegmentWasAdded BuildSegment(
        int id,
        RoadNodeWasAdded? startNode,
        RoadNodeWasAdded? endNode,
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2 status,
        RoadSegmentGeometryDrawMethodV2? geometryDrawMethod = null)
    {
        var template = TestData.Segment1Added;

        return template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode?.RoadNodeId,
            EndNodeId = endNode?.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = geometryDrawMethod ?? RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = status,
            AccessRestriction = Spanning(template.AccessRestriction, geometry),
            Category = Spanning(template.Category, geometry),
            Morphology = Spanning(template.Morphology, geometry),
            StreetNameId = Spanning(template.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(template.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(template.SurfaceType, geometry),
            CarTrafficDirection = Spanning(template.CarTrafficDirection, geometry),
            BikeTrafficDirection = Spanning(template.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = Spanning(template.PedestrianTrafficDirection, geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
    }

    private ScopedRoadNetwork BuildNetwork(
        RoadNodeWasAdded[] nodes,
        RoadSegmentWasAdded[] segments,
        GradeSeparatedJunctionWasAdded[]? gradeSeparatedJunctions = null,
        GradeJunctionWasAdded[]? gradeJunctions = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray(),
            (gradeSeparatedJunctions ?? []).Select(x => GradeSeparatedJunction.Create(x).WithoutChanges()).ToArray(),
            (gradeJunctions ?? []).Select(x => GradeJunction.Create(x).WithoutChanges()).ToArray()).WithoutChanges();
    }

    // A realized segment running north from (100,0), on its own: both its road nodes carry nothing else.
    private ScopedRoadNetwork BuildNetworkWithASegmentOnItsOwn(
        RoadSegmentStatusV2? status = null,
        RoadSegmentGeometryDrawMethodV2? drawMethod = null)
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, northNode],
            [BuildSegment(CorrectedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), status ?? RoadSegmentStatusV2.Gerealiseerd, drawMethod)]);
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    private RoadNetworkChangeResult Act(ScopedRoadNetwork roadNetwork, bool mayModifyMeasuredRoadSegments = true, int roadSegmentId = CorrectedSegmentId)
    {
        return roadNetwork.CorrectRoadSegmentFromRealizedToPlanned(
            new RoadSegmentId(roadSegmentId),
            mayModifyMeasuredRoadSegments,
            IdGenerator(),
            TestData.Provenance);
    }

    [Fact]
    public void WhenTheSegmentIsCorrected_ThenItBecomesPlannedAndGivesUpItsRoadNodes()
    {
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(CorrectedSegmentId)];
        roadSegment.Status.Should().Be(RoadSegmentStatusV2.Gepland);
        roadSegment.StartNodeId.Should().BeNull();
        roadSegment.EndNodeId.Should().BeNull();
    }

    [Fact]
    public void WhenTheSegmentIsCorrected_ThenARoadSegmentWasCorrectedFromRealizedToPlannedEventIsRecorded()
    {
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(CorrectedSegmentId)];
        roadSegment.GetChanges().OfType<RoadSegmentWasModified>().Should().BeEmpty();

        // The nodes it came loose from are named, so a reader can tell what it was hooked onto.
        var @event = roadSegment.GetChanges().OfType<RoadSegmentWasCorrectedFromRealizedToPlanned>().Should().ContainSingle().Which;
        @event.RoadSegmentId.Should().Be(new RoadSegmentId(CorrectedSegmentId));
        @event.PreviousStartNodeId.Should().Be(new RoadNodeId(10));
        @event.PreviousEndNodeId.Should().Be(new RoadNodeId(11));
    }

    [Fact]
    public void WhenTheGeometryAndAttributesAreUntouched_ThenTheyStayExactlyAsTheyWere()
    {
        // The segment is still drawn where it was drawn; it simply is not part of the network any more.
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(CorrectedSegmentId)];
        var geometryBefore = roadSegment.Geometry;
        var attributesBefore = roadSegment.Attributes;

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        roadSegment.Geometry.Should().Be(geometryBefore);
        roadSegment.Attributes.Should().Be(attributesBefore);
    }

    [Fact]
    public void WhenARoadNodeIsLeftCarryingNothing_ThenItIsRemoved()
    {
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadNodes.Removed.Should().BeEquivalentTo([new RoadNodeId(10), new RoadNodeId(11)]);
        roadNetwork.RoadNodes[new RoadNodeId(10)].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenARoadNodeStillCarriesAnotherSegment_ThenItSurvivesAndIsRetyped()
    {
        // The shared node at (100,0) carries three segments while this one is realized, so it is an 'echte knoop'.
        // Once this one comes loose two are left, which makes it a 'validatieknoop'.
        var sharedNode = BuildNode(10, 100, 0, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 0, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [sharedNode, northNode, westNode, eastNode],
            [
                BuildSegment(CorrectedSegmentId, sharedNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, sharedNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondNeighbourSegmentId, sharedNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();

        roadNetwork.RoadNodes[new RoadNodeId(10)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadNodes[new RoadNodeId(10)].Type.Should().Be(RoadNodeTypeV2.Validatieknoop);

        // The far end carried nothing else, so that one does go.
        roadNetwork.RoadNodes[new RoadNodeId(11)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheNeighboursAreLeftMeetingAtTheNode_ThenTheyAreNotMergedAway()
    {
        // Two realized roads meeting at the node this segment lets go of. The network-wide rules would take that as
        // licence to merge them into one; an editing action names one segment and leaves the others alone.
        var sharedNode = BuildNode(10, 100, 0, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 0, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [sharedNode, northNode, westNode, eastNode],
            [
                BuildSegment(CorrectedSegmentId, sharedNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, sharedNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondNeighbourSegmentId, sharedNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Removed.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(NeighbourSegmentId)].IsRemoved.Should().BeFalse();
        roadNetwork.RoadSegments[new RoadSegmentId(SecondNeighbourSegmentId)].IsRemoved.Should().BeFalse();
    }

    [Fact]
    public void WhenTheSegmentTakesPartInAGradeJunction_ThenTheJunctionGoesWithIt()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 40, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 40, RoadNodeTypeV2.Eindknoop);

        var gradeJunction = new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(CorrectedSegmentId),
            RoadSegmentId2 = new RoadSegmentId(NeighbourSegmentId),
            Geometry = JunctionGeometry.Create(new Point(new Coordinate(100, 40)) { SRID = WellknownSrids.Lambert08 }),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetwork(
            [southNode, northNode, westNode, eastNode],
            [
                BuildSegment(CorrectedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 40), (200, 40)), RoadSegmentStatusV2.Gerealiseerd)
            ],
            gradeJunctions: [gradeJunction]);

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Removed.Should().ContainSingle()
            .Which.Should().Be(new GradeJunctionId(1));
        roadNetwork.GradeJunctions[new GradeJunctionId(1)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheSegmentTakesPartInAGradeSeparatedJunction_ThenTheJunctionGoesWithIt()
    {
        var southNode = BuildNode(10, 100, 0, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(11, 100, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(12, 0, 40, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(13, 200, 40, RoadNodeTypeV2.Eindknoop);

        var junction = new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            UpperRoadSegmentId = new RoadSegmentId(CorrectedSegmentId),
            LowerRoadSegmentId = new RoadSegmentId(NeighbourSegmentId),
            Type = GradeSeparatedJunctionTypeV2.NietGekend,
            Geometry = JunctionGeometry.Create(new Point(new Coordinate(100, 40)) { SRID = WellknownSrids.Lambert08 }),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetwork(
            [southNode, northNode, westNode, eastNode],
            [
                BuildSegment(CorrectedSegmentId, southNode, northNode, BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 40), (200, 40)), RoadSegmentStatusV2.Gerealiseerd)
            ],
            gradeSeparatedJunctions: [junction]);

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeSeparatedJunctions.Removed.Should().ContainSingle()
            .Which.Should().Be(new GradeSeparatedJunctionId(1));
        roadNetwork.GradeSeparatedJunctions[new GradeSeparatedJunctionId(1)].IsRemoved.Should().BeTrue();
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void WhenTheStatusIsNotGerealiseerd_ThenError(string status)
    {
        // VAL-4
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn(RoadSegmentStatusV2.Parse(status));

        var result = Act(roadNetwork);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentCorrectFromRealizedToPlannedStatusNotValid.ProblemCode.ToString());
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayNotTouchThose_ThenError()
    {
        // VAL-5
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn(drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, mayModifyMeasuredRoadSegments: false);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentMeasuredNotAllowed.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(CorrectedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayTouchThose_ThenItIsCorrected()
    {
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn(drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, mayModifyMeasuredRoadSegments: true);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(CorrectedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentDoesNotExist_ThenNotFound()
    {
        // VAL-2
        var roadNetwork = BuildNetworkWithASegmentOnItsOwn();

        var result = Act(roadNetwork, roadSegmentId: 999);

        result.Problems.Select(x => x.Reason).Should().Contain(ProblemCode.RoadSegment.NotFound.ToString());
    }
}
