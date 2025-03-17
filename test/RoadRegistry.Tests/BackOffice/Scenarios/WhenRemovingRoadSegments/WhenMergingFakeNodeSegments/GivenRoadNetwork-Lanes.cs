namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments.WhenMergingFakeNodeSegments;

using Framework.Testing;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using LineString = NetTopologySuite.Geometries.LineString;
using RoadSegmentLaneAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentLaneAttributes;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;

public partial class GivenRoadNetwork
{
    [Fact]
    public async Task WithDifferentLanes_ThenLanesAreNotMerged()
    {
        var w5LineString = GeometryTranslator.Translate(W5.Geometry);
        W5.Lanes =
        [
            new RoadSegmentLaneAttributes
            {
                AttributeId = 1,
                Direction = RoadSegmentLaneDirection.Unknown,
                Count = 1,
                FromPosition = 0,
                ToPosition = (decimal)w5LineString.Length,
                AsOfGeometryVersion = 1
            },
        ];

        var w6LineString = GeometryTranslator.Translate(W6.Geometry);
        W6.Lanes =
        [
            new RoadSegmentLaneAttributes
            {
                AttributeId = 1,
                Direction = RoadSegmentLaneDirection.Forward,
                Count = 2,
                FromPosition = 0,
                ToPosition = 0.5M,
                AsOfGeometryVersion = 1
            },
            new RoadSegmentLaneAttributes
            {
                AttributeId = 2,
                Direction = RoadSegmentLaneDirection.Backward,
                Count = 3,
                FromPosition = 0.5M,
                ToPosition = (decimal)w6LineString.Length,
                AsOfGeometryVersion = 1
            },
        ];

        var mergedSegmentId = 11;

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
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
                    Code = W5.MaintenanceAuthority.Code
                },
                Geometry = GeometryTranslator.Translate(new MultiLineString([new LineString([
                    new(0, 0), new(1, 1), new(1, 0)
                ])])),
                GeometryVersion = 1,
                Lanes =
                [
                    new()
                    {
                        AttributeId = 3,
                        AsOfGeometryVersion = 1,
                        FromPosition = 0,
                        ToPosition = 1.4142135623731M,
                        Direction = RoadSegmentLaneDirection.Unknown,
                        Count = 1
                    },
                    new()
                    {
                        AttributeId = 4,
                        AsOfGeometryVersion = 1,
                        FromPosition = 1.4142135623731M,
                        ToPosition = 1.9142135623731M,
                        Direction = RoadSegmentLaneDirection.Forward,
                        Count = 2
                    },
                    new()
                    {
                        AttributeId = 5,
                        AsOfGeometryVersion = 1,
                        FromPosition = 1.9142135623731M,
                        ToPosition = 2.4142135623731M,
                        Direction = RoadSegmentLaneDirection.Backward,
                        Count = 3
                    }
                ],
                Surfaces = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Type = RoadSegmentSurfaceType.Unknown
                }],
                Widths = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Width = RoadSegmentWidth.Unknown
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
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(_command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
