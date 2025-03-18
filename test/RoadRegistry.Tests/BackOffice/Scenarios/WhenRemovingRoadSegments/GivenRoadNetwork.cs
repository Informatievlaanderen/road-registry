namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments
{
    using Framework.Testing;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;
    using Problem = RoadRegistry.BackOffice.Messages.Problem;
    using ProblemSeverity = RoadRegistry.BackOffice.Messages.ProblemSeverity;
    using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

    public class GivenRoadNetwork: RemoveRoadSegmentsTestBase
    {
        public GivenRoadNetwork(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task WhenSegmentNotFound_ThenProblem()
        {
            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [1]
            };

            var command = new ChangeRoadNetworkBuilder(TestData)
                .WithRemoveRoadSegments(removeRoadSegments.Ids)
                .Build();

            var expected = new RoadNetworkChangesRejectedBuilder(TestData)
                .WithClock(Clock)
                .WithTransactionId(1)
                .WithRemoveRoadSegments(removeRoadSegments, [
                    new Problem
                    {
                        Severity = ProblemSeverity.Error,
                        Reason = "RoadSegmentNotFound",
                        Parameters = [
                            new()
                            {
                                Name = "SegmentId",
                                Value = "1"
                            }
                        ]
                    }
                ])
                .Build();

            await Run(scenario =>
                scenario
                    .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                    .When(command)
                    .Then(RoadNetworks.Stream, expected)
            );
        }

        [Theory]
        [InlineData(nameof(RoadSegmentCategory.EuropeanMainRoad), "europese hoofdweg")]
        [InlineData(nameof(RoadSegmentCategory.FlemishMainRoad), "vlaamse hoofdweg")]
        [InlineData(nameof(RoadSegmentCategory.MainRoad), "hoofdweg")]
        [InlineData(nameof(RoadSegmentCategory.PrimaryRoadI), "primaire weg I")]
        [InlineData(nameof(RoadSegmentCategory.PrimaryRoadII), "primaire weg II")]
        public async Task WhenSegmentHasNotAllowedCategory_ThenProblem(string category, string categoryTranslation)
        {
            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [W1.Id]
            };

            W1.Category = category;

            var command = new ChangeRoadNetworkBuilder(TestData)
                .WithRemoveRoadSegments(removeRoadSegments.Ids)
                .Build();

            var expected = new RoadNetworkChangesRejectedBuilder(TestData)
                .WithClock(Clock)
                .WithTransactionId(2)
                .WithRemoveRoadSegments(removeRoadSegments, [
                    new Problem
                    {
                        Severity = ProblemSeverity.Error,
                        Reason = "RoadSegmentNotRemovedBecauseCategoryIsInvalid",
                        Parameters = [
                            new()
                            {
                                Name = "Identifier",
                                Value = "1"
                            },
                            new()
                            {
                                Name = "Category",
                                Value = categoryTranslation
                            }
                        ]
                    }
                ])
                .Build();

            await Run(scenario =>
                scenario
                    .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                    .Given(RoadNetworks.Stream, InitialRoadNetwork)
                    .When(command)
                    .Then(RoadNetworks.Stream, expected)
            );
        }

        [Fact]
        public async Task WhenCreatingAnIsland_ThenProblem()
        {
            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [W2.Id, W5.Id, W6.Id, W9.Id]
            };

            var command = new ChangeRoadNetworkBuilder(TestData)
                .WithRemoveRoadSegments(removeRoadSegments.Ids)
                .Build();

            var expected = new RoadNetworkChangesRejectedBuilder(TestData)
                .WithClock(Clock)
                .WithTransactionId(2)
                .WithRemoveRoadSegments(removeRoadSegments, [
                    new Problem
                    {
                        Severity = ProblemSeverity.Error,
                        Reason = "RoadNetworkDisconnected",
                        Parameters = [
                            new()
                            {
                                Name = "StartNodeId",
                                Value = "2"
                            },
                            new()
                            {
                                Name = "EndNodeId",
                                Value = "3"
                            }
                        ]
                    },
                    new Problem
                    {
                        Severity = ProblemSeverity.Error,
                        Reason = "RoadNetworkDisconnected",
                        Parameters = [
                            new()
                            {
                                Name = "StartNodeId",
                                Value = "2"
                            },
                            new()
                            {
                                Name = "EndNodeId",
                                Value = "6"
                            }
                        ]
                    }
                ])
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
}
