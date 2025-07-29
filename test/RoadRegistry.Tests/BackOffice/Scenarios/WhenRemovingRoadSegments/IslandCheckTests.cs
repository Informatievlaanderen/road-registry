namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments
{
    using FluentAssertions;
    using Framework.Testing;
    using Moq;
    using NetTopologySuite.Geometries;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.BackOffice.Messages;
    using Problem = RoadRegistry.BackOffice.Messages.Problem;
    using ProblemSeverity = RoadRegistry.BackOffice.Messages.ProblemSeverity;
    using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

    public class IslandCheckTests : RemoveRoadSegmentsTestBase
    {
        public IslandCheckTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
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
                        Parameters =
                        [
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
                        Parameters =
                        [
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

        [Fact]
        public async Task WhenNodeIsOutsideSegmentBoundingBox_ThenNodeIsIncludedInScopedView()
        {
            K5.Geometry.Point.X = -1;
            K5.Geometry.Point.Y = 0;

            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [W5.Id]
            };

            var rootView = ImmutableRoadNetworkView.Empty.RestoreFromEvent(InitialRoadNetwork);

            var afterVerificationContext = await BuildAfterVerificationContext(rootView, removeRoadSegments);
            afterVerificationContext.BeforeView.Nodes.Should().ContainKey(new RoadNodeId(K5.Id));
        }

        [Fact]
        public async Task WhenNodeOfMergedSegmentIsOutsideSegmentBoundingBox_ThenNodeIsIncludedInScopedView()
        {
            K5.Geometry.Point.X = -1;
            K5.Geometry.Point.Y = 0;

            W9.MaintenanceAuthority.Code = W10.MaintenanceAuthority.Code;

            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [W6.Id, W7.Id]
            };

            var rootView = ImmutableRoadNetworkView.Empty.RestoreFromEvent(InitialRoadNetwork);

            var afterVerificationContext = await BuildAfterVerificationContext(rootView, removeRoadSegments);
            afterVerificationContext.BeforeView.Nodes.Should().ContainKey(new RoadNodeId(K5.Id));
        }

        [Fact]
        public async Task WhenNoNodesAreRemoved_ThenShouldFindDirectConnection()
        {
            K2.Geometry.Point.X = 1;
            K2.Geometry.Point.Y = 2;

            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [W6.Id]
            };

            var rootView = ImmutableRoadNetworkView.Empty.RestoreFromEvent(InitialRoadNetwork);

            var expectedScope = new Envelope(new Coordinate(0, 0), new Coordinate(2, 1));

            var afterVerificationContext = await BuildAfterVerificationContext(rootView, removeRoadSegments);
            afterVerificationContext.BeforeView.Scope.Should().BeEquivalentTo(expectedScope);
            afterVerificationContext.BeforeView.Nodes.Should().ContainKey(new RoadNodeId(K2.Id));

            var connections = afterVerificationContext.GetPossibleConnectionsForRemovedSegments(removeRoadSegments.Ids.Select(x => new RoadSegmentId(x)).ToArray());

            var expectedConnections = new Dictionary<RoadNodeId, IEnumerable<RoadNodeId>>
            {
                { new RoadNodeId(K2.Id), [new RoadNodeId(K6.Id)] }
            };

            connections.Should().BeEquivalentTo(expectedConnections);
        }

        [Fact]
        public async Task WhenAtLeastOneNodeIsRemoved_ThenShouldFindConnectionViaLinkedSegments()
        {
            K5.Geometry.Point.X = -1;
            K5.Geometry.Point.Y = 0;

            W9.MaintenanceAuthority.Code = W10.MaintenanceAuthority.Code;

            var removeRoadSegments = new RemoveRoadSegments
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                Ids = [W6.Id, W7.Id]
            };

            var rootView = ImmutableRoadNetworkView.Empty.RestoreFromEvent(InitialRoadNetwork);

            var expectedScope = new Envelope(new Coordinate(0, 0), new Coordinate(3, 1));

            var afterVerificationContext = await BuildAfterVerificationContext(rootView, removeRoadSegments);
            afterVerificationContext.BeforeView.Scope.Should().BeEquivalentTo(expectedScope);
            afterVerificationContext.BeforeView.Nodes.Should().ContainKey(new RoadNodeId(K5.Id));

            var connections = afterVerificationContext.GetPossibleConnectionsForRemovedSegments(removeRoadSegments.Ids.Select(x => new RoadSegmentId(x)).ToArray());

            var expectedConnections = new Dictionary<RoadNodeId, IEnumerable<RoadNodeId>>
            {
                { new RoadNodeId(K2.Id), [new RoadNodeId(K3.Id), new RoadNodeId(K5.Id), new RoadNodeId(K7.Id)] },
                { new RoadNodeId(K3.Id), [new RoadNodeId(K5.Id), new RoadNodeId(K7.Id)] }
            };

            connections.Should().BeEquivalentTo(expectedConnections);
        }

        private async Task<AfterVerificationContext> BuildAfterVerificationContext(IRoadNetworkView rootView, RemoveRoadSegments removeRoadSegments)
        {
            var command = new ChangeRoadNetworkBuilder(TestData)
                .WithRemoveRoadSegments(removeRoadSegments.Ids)
                .Build();

            var translator = new RequestedChangeTranslator(
                Mock.Of<IRoadNetworkIdProvider>(),
                _ => RoadNodeVersion.Initial,
                (_, _) => Task.FromResult(RoadSegmentVersion.Initial),
                (_, _, _) => Task.FromResult(GeometryVersion.Initial)
            );
            var requestedChanges = await translator.Translate(((ChangeRoadNetwork)command.Body).Changes, Mock.Of<IOrganizations>(), CancellationToken.None);

            var scope = requestedChanges.DeriveScopeFromChanges(rootView);

            return new AfterVerificationContext(
                rootView,
                rootView.CreateScopedView(scope),
                rootView.With(requestedChanges).CreateScopedView(scope),
                Mock.Of<IRequestedChangeIdentityTranslator>(),
                VerificationContextTolerances.Default,
                Mock.Of<IOrganizations>());
        }
    }
}
