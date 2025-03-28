namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments.WhenMergingFakeNodeSegments;

using AutoFixture;
using Framework.Testing;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using LineString = NetTopologySuite.Geometries.LineString;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;

public partial class GivenIdenticalSegments
{
    [Fact]
    public async Task WhenSegmentsHaveDifferentAttribute_EuropeanRoads_ThenNoMerge()
    {
        var attributeId1 = Fixture.Create<AttributeId>();

        var initialRoads = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = attributeId1,
                SegmentId = W5.Id,
                SegmentVersion = Fixture.Create<RoadSegmentVersion>(),
                Number = "E40"
            })
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = Fixture.Create<AttributeId>(),
                SegmentId = W6.Id,
                SegmentVersion = Fixture.Create<RoadSegmentVersion>(),
                Number = "E19"
            })
            .WithTransactionId(2)
            .Build();

        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id);

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(3)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeModified(new()
            {
                Id = K2.Id,
                Type = RoadNodeType.FakeNode,
                Version = K2.Version + 1,
                Geometry = K2.Geometry
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .Given(RoadNetworks.Stream, initialRoads)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WithEuropeanRoads_ThenEuropeanRoadsAreLinkedToMergedSegment()
    {
        var attributeId1 = new AttributeId(1);
        var attributeId2 = new AttributeId(2);

        var initialRoads = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = attributeId1,
                SegmentId = W5.Id,
                SegmentVersion = Fixture.Create<RoadSegmentVersion>(),
                Number = "E40"
            })
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = attributeId2,
                SegmentId = W5.Id,
                SegmentVersion = Fixture.Create<RoadSegmentVersion>(),
                Number = "E19"
            })
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = new AttributeId(3),
                SegmentId = W6.Id,
                SegmentVersion = Fixture.Create<RoadSegmentVersion>(),
                Number = "E40"
            })
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = new AttributeId(4),
                SegmentId = W6.Id,
                SegmentVersion = Fixture.Create<RoadSegmentVersion>(),
                Number = "E19"
            })
            .WithTransactionId(2)
            .Build();

        var command = BuildRemoveRoadSegmentsCommand(W1.Id, W2.Id);

        var mergedSegmentId = 11;

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(3)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeRemoved(K2.Id)
            .WithRoadSegmentAdded(new()
            {
                Id = mergedSegmentId,
                TemporaryId = 11,
                Version = 1,
                StartNodeId = K5.Id,
                EndNodeId = K6.Id,
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                AccessRestriction = W5.AccessRestriction,
                Category = W5.Category,
                Status = W5.Status,
                Morphology = W5.Morphology,
                MaintenanceAuthority = new MaintenanceAuthority
                {
                    Code = W5.MaintenanceAuthority.Code,
                    Name = W5.MaintenanceAuthority.Name
                },
                Geometry = GeometryTranslator.Translate(new MultiLineString([new LineString([
                    new(0, 0), new(1, 1), new(1, 0)
                ])])),
                GeometryVersion = 1,
                Lanes = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Direction = RoadSegmentLaneDirection.Forward,
                    Count = 1
                }],
                Surfaces = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Type = RoadSegmentSurfaceType.SolidSurface
                }],
                Widths = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Width = new RoadSegmentWidth(3)
                }],
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = W5.LeftSide.StreetNameId
                },
                RightSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = W5.RightSide.StreetNameId
                }
            })
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = 5,
                TemporaryAttributeId = attributeId1,
                SegmentId = mergedSegmentId,
                SegmentVersion = 1,
                Number = "E40"
            })
            .WithRoadSegmentAddedToEuropeanRoad(new()
            {
                AttributeId = 6,
                TemporaryAttributeId = attributeId2,
                SegmentId = mergedSegmentId,
                SegmentVersion = 1,
                Number = "E19"
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .GivenOrganization(W5.MaintenanceAuthority)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .Given(RoadNetworks.Stream, initialRoads)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
