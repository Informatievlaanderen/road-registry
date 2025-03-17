namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments.WhenMergingFakeNodeSegments;

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
    public async Task WithJunctions_ThenJunctionsAreLinkedToMergedSegment()
    {
        var j1 = new GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = 1,
            Type = GradeSeparatedJunctionType.Unknown,
            LowerRoadSegmentId = W5.Id,
            UpperRoadSegmentId = W9.Id
        };
        var j2 = new GradeSeparatedJunctionAdded
        {
            Id = 2,
            TemporaryId = 2,
            Type = GradeSeparatedJunctionType.Unknown,
            LowerRoadSegmentId = W9.Id,
            UpperRoadSegmentId = W6.Id
        };
        var initialJunctions = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithGradeSeparatedJunctionAdded(j1)
            .WithGradeSeparatedJunctionAdded(j2)
            .WithTransactionId(2)
            .Build();

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
                Id = 11,
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
                Lanes = [new()
                {
                    AttributeId = 2,
                    AsOfGeometryVersion = 1,
                    FromPosition = 0,
                    ToPosition = 2.4142135623731M,
                    Direction = RoadSegmentLaneDirection.Unknown,
                    Count = 1
                }],
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
            .WithGradeSeparatedJunctionModified(new()
            {
                Id = j1.Id,
                Type = j1.Type,
                LowerRoadSegmentId = 11,
                UpperRoadSegmentId = W9.Id
            })
            .WithGradeSeparatedJunctionModified(new()
            {
                Id = j2.Id,
                Type = j2.Type,
                LowerRoadSegmentId = W9.Id,
                UpperRoadSegmentId = 11
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .Given(RoadNetworks.Stream, initialJunctions)
                .When(_command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
