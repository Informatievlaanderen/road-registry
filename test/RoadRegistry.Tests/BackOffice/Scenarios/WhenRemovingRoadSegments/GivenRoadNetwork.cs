namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using AutoFixture;
using Framework.Testing;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using LineString = RoadRegistry.BackOffice.Messages.LineString;
using Point = RoadRegistry.BackOffice.Messages.Point;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;
using RoadSegmentSurfaceAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSurfaceAttributes;
using RoadSegmentWidthAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentWidthAttributes;

public class GivenRoadNetwork : RoadNetworkTestBase
{
    private readonly Fixture _fixture;
    private readonly RoadNodeAdded _k1;
    private readonly RoadNodeAdded _k2;
    private readonly RoadNodeAdded _k3;
    private readonly RoadNodeAdded _k4;
    private readonly RoadNodeAdded _k5;
    private readonly RoadNodeAdded _k6;
    private readonly RoadNodeAdded _k7;
    private readonly RoadSegmentAdded _w1;
    private readonly RoadSegmentAdded _w2;
    private readonly RoadSegmentAdded _w3;
    private readonly RoadSegmentAdded _w4;
    private readonly RoadSegmentAdded _w5;
    private readonly RoadSegmentAdded _w6;
    private readonly RoadSegmentAdded _w7;
    private readonly RoadSegmentAdded _w8;
    private readonly RoadSegmentAdded _w9;
    private readonly RoadSegmentAdded _w10;
    private RoadNetworkChangesAccepted _initialRoadNetwork;

    public GivenRoadNetwork(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _fixture = TestData.ObjectProvider;

        _k1 = CreateRoadNode(1, RoadNodeType.EndNode, 0, 1);
        _k2 = CreateRoadNode(2, RoadNodeType.RealNode, 1, 1);
        _k3 = CreateRoadNode(3, RoadNodeType.RealNode, 2, 1);
        _k4 = CreateRoadNode(4, RoadNodeType.TurningLoopNode, 3, 1);
        _k5 = CreateRoadNode(5, RoadNodeType.FakeNode, 0, 0);
        _k6 = CreateRoadNode(6, RoadNodeType.MiniRoundabout, 1, 0);
        _k7 = CreateRoadNode(7, RoadNodeType.FakeNode, 2, 0);

        _w1 = CreateRoadSegment(1, _k1, _k2);
        _w2 = CreateRoadSegment(2, _k2, _k3);
        _w3 = CreateRoadSegment(3, _k3, _k4);
        _w4 = CreateRoadSegment(4, _k4, _k3);
        _w5 = CreateRoadSegment(5, _k5, _k2);
        _w6 = CreateRoadSegment(6, _k2, _k6);
        _w7 = CreateRoadSegment(7, _k6, _k3);
        _w8 = CreateRoadSegment(8, _k3, _k7);
        _w9 = CreateRoadSegment(9, _k5, _k6);
        _w10 = CreateRoadSegment(10, _k6, _k7);

        _initialRoadNetwork = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadNodeAdded(_k1)
            .WithRoadNodeAdded(_k2)
            .WithRoadNodeAdded(_k3)
            .WithRoadNodeAdded(_k4)
            .WithRoadNodeAdded(_k5)
            .WithRoadNodeAdded(_k6)
            .WithRoadNodeAdded(_k7)
            .WithRoadSegmentAdded(_w1)
            .WithRoadSegmentAdded(_w2)
            .WithRoadSegmentAdded(_w3)
            .WithRoadSegmentAdded(_w4)
            .WithRoadSegmentAdded(_w5)
            .WithRoadSegmentAdded(_w6)
            .WithRoadSegmentAdded(_w7)
            .WithRoadSegmentAdded(_w8)
            .WithRoadSegmentAdded(_w9)
            .WithRoadSegmentAdded(_w10)
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
        var lineString = new NetTopologySuite.Geometries.LineString([
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

    [Fact]
    public async Task WhenRemovingSegmentWithEndNode()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [_w1.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentsRemoved(new RoadSegmentsRemoved
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                RemovedRoadSegmentIds = [_w1.Id],
                RemovedRoadNodeIds = [_k1.Id],
                ChangedRoadNodes = [],
                RemovedGradeSeparatedJunctionIds = []
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, _initialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
