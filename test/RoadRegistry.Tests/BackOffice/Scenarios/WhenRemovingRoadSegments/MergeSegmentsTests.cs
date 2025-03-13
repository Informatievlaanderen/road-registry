namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;
using RoadSegmentSideAttributes = RoadRegistry.BackOffice.Messages.RoadSegmentSideAttributes;

public class MergeSegmentsTests : RemoveRoadSegmentsTestBase
{
    public MergeSegmentsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    //TODO-pr test cases
    //TODO-pr test cases merge geometrie
    //TODO-pr test cases merge lane/... attributes

    [Fact]
    public async Task WhenEndingUpWithFakeNodeAndIdenticalConnectedSegments_ThenNodeIsRemovedAndSegmentsMerged()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W1.Id, W2.Id]
        };

        W5.MaintenanceAuthority.Code = "A";
        W6.MaintenanceAuthority.Code = "A";

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeRemoved(K2.Id)
            .WithRoadSegmentAdded(new()
            {
                Id = 2147483647,
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
                Geometry = null,
                    /*"MultiLineString": [
            {
              "Measures": [
                0.0,
                1.4142135623730951,
                2.414213562373095
              ],
              "Points": [
                {
                  "X": 0.0,
                  "Y": 0.0
                },
                {
                  "X": 1.0,
                  "Y": 1.0
                },
                {
                  "X": 1.0,
                  "Y": 0.0
                }
              ]
            }
          ],
          "SpatialReferenceSystemIdentifier": 31370*/
                GeometryVersion = 1,
                Lanes = [],
                Surfaces = [],
                Widths = [],
                LeftSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = null
                },
                RightSide = new RoadSegmentSideAttributes
                {
                    StreetNameId = null
                }
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
