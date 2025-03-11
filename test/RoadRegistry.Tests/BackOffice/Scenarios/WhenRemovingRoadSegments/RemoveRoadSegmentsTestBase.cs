namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = RoadRegistry.BackOffice.Messages.Point;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public abstract class RemoveRoadSegmentsTestBase : RoadNetworkTestBase
{
    private readonly Fixture _fixture;

    protected readonly RoadNodeAdded K1;
    protected readonly RoadNodeAdded K2;
    protected readonly RoadNodeAdded K3;
    protected readonly RoadNodeAdded K4;
    protected readonly RoadNodeAdded K5;
    protected readonly RoadNodeAdded K6;
    protected readonly RoadNodeAdded K7;
    protected readonly RoadSegmentAdded W1;
    protected readonly RoadSegmentAdded W2;
    protected readonly RoadSegmentAdded W3;
    protected readonly RoadSegmentAdded W4;
    protected readonly RoadSegmentAdded W5;
    protected readonly RoadSegmentAdded W6;
    protected readonly RoadSegmentAdded W7;
    protected readonly RoadSegmentAdded W8;
    protected readonly RoadSegmentAdded W9;
    protected readonly RoadSegmentAdded W10;

    protected readonly RoadNetworkChangesAccepted InitialRoadNetwork;

    protected RemoveRoadSegmentsTestBase(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _fixture = TestData.ObjectProvider;

        K1 = CreateRoadNode(1, RoadNodeType.EndNode, 0, 1);
        K2 = CreateRoadNode(2, RoadNodeType.RealNode, 1, 1);
        K3 = CreateRoadNode(3, RoadNodeType.RealNode, 2, 1);
        K4 = CreateRoadNode(4, RoadNodeType.TurningLoopNode, 3, 1);
        K5 = CreateRoadNode(5, RoadNodeType.FakeNode, 0, 0);
        K6 = CreateRoadNode(6, RoadNodeType.MiniRoundabout, 1, 0);
        K7 = CreateRoadNode(7, RoadNodeType.FakeNode, 2, 0);

        W1 = CreateRoadSegment(1, K1, K2);
        W2 = CreateRoadSegment(2, K2, K3);
        W3 = CreateRoadSegment(3, K3, K4);
        W4 = CreateRoadSegment(4, K4, K3);
        W5 = CreateRoadSegment(5, K5, K2);
        W6 = CreateRoadSegment(6, K2, K6);
        W7 = CreateRoadSegment(7, K6, K3);
        W8 = CreateRoadSegment(8, K3, K7);
        W9 = CreateRoadSegment(9, K5, K6);
        W10 = CreateRoadSegment(10, K6, K7);

        InitialRoadNetwork = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadNodeAdded(K1)
            .WithRoadNodeAdded(K2)
            .WithRoadNodeAdded(K3)
            .WithRoadNodeAdded(K4)
            .WithRoadNodeAdded(K5)
            .WithRoadNodeAdded(K6)
            .WithRoadNodeAdded(K7)
            .WithRoadSegmentAdded(W1)
            .WithRoadSegmentAdded(W2)
            .WithRoadSegmentAdded(W3)
            .WithRoadSegmentAdded(W4)
            .WithRoadSegmentAdded(W5)
            .WithRoadSegmentAdded(W6)
            .WithRoadSegmentAdded(W7)
            .WithRoadSegmentAdded(W8)
            .WithRoadSegmentAdded(W9)
            .WithRoadSegmentAdded(W10)
            .Build();
    }

    private RoadNodeAdded CreateRoadNode(int id, RoadNodeType roadNodeType, int x, int y)
    {
        return new RoadNodeAdded
        {
            Id = id,
            TemporaryId = id,
            Geometry = new RoadNodeGeometry { Point = new Point { X = x, Y = y }, SpatialReferenceSystemIdentifier = 31370 },
            Type = roadNodeType,
            Version = 1
        };
    }

    private RoadSegmentAdded CreateRoadSegment(
        int id,
        RoadNodeAdded startNode,
        RoadNodeAdded endNode)
    {
        var lineString = new LineString([
            GeometryTranslator.Translate(startNode.Geometry).Coordinate,
            GeometryTranslator.Translate(endNode.Geometry).Coordinate]);

        var roadSegmentGeometry = GeometryTranslator.Translate(lineString.ToMultiLineString());

        return new RoadSegmentAdded
        {
            Id = id,
            Version = 1,
            TemporaryId = id,
            StartNodeId = startNode.Id,
            EndNodeId = endNode.Id,
            Geometry = roadSegmentGeometry,
            GeometryVersion = 1,
            MaintenanceAuthority = new MaintenanceAuthority
            {
                Code = TestData.ChangedByOrganization,
                Name = TestData.ChangedByOrganizationName
            },
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Morphology = RoadSegmentMorphology.Unknown,
            Status = RoadSegmentStatus.Unknown,
            Category = RoadSegmentCategory.Unknown,
            AccessRestriction = RoadSegmentAccessRestriction.PublicRoad,
            LeftSide = new RoadSegmentSideAttributes
            {
                StreetNameId = StreetNameLocalId.NotApplicable
            },
            RightSide = new RoadSegmentSideAttributes
            {
                StreetNameId = StreetNameLocalId.NotApplicable
            },
            Lanes =
            [
                new RoadSegmentLaneAttributes
                {
                    AttributeId = 1,
                    Direction = RoadSegmentLaneDirection.Unknown,
                    Count = 1,
                    FromPosition = 0,
                    ToPosition = (decimal)lineString.Length,
                    AsOfGeometryVersion = 1
                }
            ],
            Widths =
            [
                new RoadSegmentWidthAttributes
                {
                    AttributeId = 1,
                    Width = _fixture.Create<RoadSegmentWidth>(),
                    FromPosition = 0,
                    ToPosition = (decimal)lineString.Length,
                    AsOfGeometryVersion = 1
                }
            ],
            Surfaces =
            [
                new RoadSegmentSurfaceAttributes
                {
                    AttributeId = 1,
                    Type = _fixture.Create<RoadSegmentSurfaceType>(),
                    FromPosition = 0,
                    ToPosition = (decimal)lineString.Length,
                    AsOfGeometryVersion = 1
                }
            ]
        };
    }
}
