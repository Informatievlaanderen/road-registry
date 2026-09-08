namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;
using RoadSegmentGeometryHelper = RoadRegistry.RoadSegment.RoadSegmentGeometryHelper;

// Joining the geometries of two roads that meet in one node. The merged road runs from the far end of segment1,
// through the node, to the far end of segment2 - so which road is handed over first decides the direction the merged
// road is digitised in, and nothing else.
public class RoadSegmentGeometryHelperMergeGeometriesTests : AggregateTestBase
{
    private const int WestNodeId = 10;
    private const int MiddleNodeId = 11;
    private const int EastNodeId = 12;

    private const int WestSegmentId = 1;
    private const int EastSegmentId = 2;

    // The two roads meeting at the middle node, each with a bend of its own so the vertices in between are visible in
    // the result rather than collapsing onto the straight line between the nodes.
    private static readonly (double X, double Y)[] WestCoordinates = [(0, 0), (50, 10), (100, 0)];
    private static readonly (double X, double Y)[] EastCoordinates = [(100, 0), (150, -10), (200, 0)];

    private static readonly Coordinate[] ExpectedCoordinates =
    [
        new(0, 0), new(50, 10), new(100, 0), new(150, -10), new(200, 0)
    ];

    private RoadNodeWasAdded BuildNode(int id, double x, double y)
    {
        return new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
            Grensknoop = false,
            Type = RoadNodeTypeV2.Validatieknoop,
            Provenance = new ProvenanceData(TestData.Provenance)
        };
    }

    private static RoadSegmentGeometry BuildGeometry(params (double X, double Y)[] coordinates)
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

    // Digitised from startNode to endNode, so flipping the two is how a road ends up drawn the other way round.
    private RoadSegment BuildSegment(int id, RoadNodeWasAdded startNode, RoadNodeWasAdded endNode, (double X, double Y)[] coordinates, bool reverse)
    {
        if (reverse)
        {
            (startNode, endNode) = (endNode, startNode);
            coordinates = coordinates.Reverse().ToArray();
        }

        var geometry = BuildGeometry(coordinates);
        var template = TestData.Segment1Added;

        var added = template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode.RoadNodeId,
            EndNodeId = endNode.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            AccessRestriction = Spanning(template.AccessRestriction, geometry),
            Category = Spanning(template.Category, geometry),
            Morphology = Spanning(template.Morphology, geometry),
            StreetNameId = Spanning(template.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(template.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(template.SurfaceType, geometry),
            CarTrafficDirection = Spanning(template.CarTrafficDirection, geometry),
            BikeTrafficDirection = Spanning(template.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>()
                .Add(RoadSegmentPedestrianTrafficDirection.None, geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        return RoadSegment.Create(added).WithoutChanges();
    }

    private (RoadSegment West, RoadSegment East, ScopedRoadNetworkChangeContext Context) Build(bool reverseWest, bool reverseEast)
    {
        var westNode = BuildNode(WestNodeId, 0, 0);
        var middleNode = BuildNode(MiddleNodeId, 100, 0);
        var eastNode = BuildNode(EastNodeId, 200, 0);

        var westSegment = BuildSegment(WestSegmentId, westNode, middleNode, WestCoordinates, reverseWest);
        var eastSegment = BuildSegment(EastSegmentId, middleNode, eastNode, EastCoordinates, reverseEast);

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            new[] { westNode, middleNode, eastNode }.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            [westSegment, eastSegment],
            [],
            []).WithoutChanges();

        return (westSegment, eastSegment, new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));
    }

    private static Coordinate[] Merge(RoadSegment segment1, RoadSegment segment2, ScopedRoadNetworkChangeContext context)
    {
        return RoadSegmentGeometryHelper
            .MergeGeometries(segment1, segment2, new RoadNodeId(MiddleNodeId), context)
            .GetSingleLineString()
            .Coordinates;
    }

    // The four ways the two roads can have been digitised. None of them may change the shape of the merged road.
    public static TheoryData<bool, bool> DigitisationDirections => new()
    {
        { false, false },
        { false, true },
        { true, false },
        { true, true }
    };

    [Theory]
    [MemberData(nameof(DigitisationDirections))]
    public void TheMergedGeometryRunsFromSegment1ThroughTheNodeToSegment2(bool reverseWest, bool reverseEast)
    {
        var (west, east, context) = Build(reverseWest, reverseEast);

        var merged = Merge(west, east, context);

        merged.Should().BeEquivalentTo(ExpectedCoordinates, options => options.WithStrictOrdering(),
            "the merged road keeps every vertex of both roads, in the order they are walked from the far end of segment1");
    }

    [Theory]
    [MemberData(nameof(DigitisationDirections))]
    public void SwappingSegment1AndSegment2_YieldsTheSameRoadDigitisedTheOtherWay(bool reverseWest, bool reverseEast)
    {
        var (west, east, context) = Build(reverseWest, reverseEast);

        var westFirst = Merge(west, east, context);
        var eastFirst = Merge(east, west, context);

        eastFirst.Should().BeEquivalentTo(westFirst.Reverse(), options => options.WithStrictOrdering(),
            "which road is handed over first decides only the direction the merged road is digitised in");
    }

    [Theory]
    [MemberData(nameof(DigitisationDirections))]
    public void TheMergedGeometryTouchesTheNodesItRunsBetween(bool reverseWest, bool reverseEast)
    {
        var (west, east, context) = Build(reverseWest, reverseEast);
        var nodes = context.RoadNetwork.RoadNodes;

        var merged = Merge(west, east, context);

        merged[0].Should().Be(nodes[new RoadNodeId(WestNodeId)].Geometry.Value.Coordinate);
        merged[^1].Should().Be(nodes[new RoadNodeId(EastNodeId)].Geometry.Value.Coordinate);
        merged.Should().Contain(nodes[new RoadNodeId(MiddleNodeId)].Geometry.Value.Coordinate,
            "the node the two roads met in stays a vertex of the merged road");
    }

    [Fact]
    public void TheMergedGeometryKeepsTheSrid()
    {
        var (west, east, context) = Build(reverseWest: false, reverseEast: false);

        var merged = RoadSegmentGeometryHelper.MergeGeometries(west, east, new RoadNodeId(MiddleNodeId), context);

        merged.SRID.Should().Be(WellknownSrids.Lambert08);
    }

    // A single merged road, not two lines that happen to share an endpoint.
    [Theory]
    [MemberData(nameof(DigitisationDirections))]
    public void TheMergedGeometryIsOneLineOfTheCombinedLength(bool reverseWest, bool reverseEast)
    {
        var (west, east, context) = Build(reverseWest, reverseEast);

        var merged = RoadSegmentGeometryHelper.MergeGeometries(west, east, new RoadNodeId(MiddleNodeId), context);

        merged.NumGeometries.Should().Be(1);
        merged.Length.Should().BeApproximately(west.Geometry.Value.Length + east.Geometry.Value.Length, 0.001);
    }
}
