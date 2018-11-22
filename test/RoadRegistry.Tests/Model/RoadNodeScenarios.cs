namespace RoadRegistry.Model
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using Aiv.Vbr.Shaperon;
    using Testing;
    using Xunit;
    using Messages;

    public class RoadNodeScenarios : RoadRegistryFixture
    {
        public RoadNodeScenarios()
        {
            Fixture.CustomizePointM();
            Fixture.CustomizePolylineM();

            Fixture.CustomizeRoadNodeId();
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_that_has_not_been_taken()
        {
            var temporaryId = Fixture.Create<RoadNodeId>();
            var geometry = GeometryTranslator.Translate(Fixture.Create<PointM>());
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            TemporaryId = temporaryId,
                            Type = RoadNodeType.FakeNode,
                            Geometry = geometry
                        }
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                TemporaryId = temporaryId,
                                Type = RoadNodeType.FakeNode,
                                Geometry = geometry
                            }
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_node_with_a_geometry_that_has_been_taken()
        {
            var geometry = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var addRoadNode = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Type = RoadNodeType.FakeNode,
                Geometry = geometry
            };
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Type = RoadNodeType.RealNode,
                                Geometry = geometry
                            }
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = addRoadNode
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            AddRoadNode = addRoadNode,
                            Reasons = new []
                            {
                                new Reason
                                {
                                    Because = "RoadNodeGeometryTaken",
                                    Parameters = new[]
                                    {
                                        new ReasonParameter
                                        {
                                            Name = "ByOtherNode",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_multiple_nodes_with_an_id_that_has_not_been_taken()
        {
            var temporaryId1 = Fixture.Create<RoadNodeId>();
            var temporaryId2 = Fixture.Create<RoadNodeId>();
            var geometry1 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var geometry2 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            TemporaryId = temporaryId1,
                            Type = RoadNodeType.FakeNode,
                            Geometry = geometry1
                        }
                    },
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            TemporaryId = temporaryId2,
                            Type = RoadNodeType.FakeNode,
                            Geometry = geometry2
                        }
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                TemporaryId = temporaryId1,
                                Type = RoadNodeType.FakeNode,
                                Geometry = geometry1
                            }
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 2,
                                TemporaryId = temporaryId2,
                                Type = RoadNodeType.FakeNode,
                                Geometry = geometry2
                            }
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_node_that_is_within_two_meters_of_another_node()
        {
            var geometry1 = Fixture.Create<PointM>();
            var random = new Random();
            var geometry2 = new PointM(
                geometry1.X + random.NextDouble() * RoadNetwork.TooCloseDistance,
                geometry1.Y + random.NextDouble() * RoadNetwork.TooCloseDistance,
                geometry1.Z + random.NextDouble() * RoadNetwork.TooCloseDistance
            );
            var addRoadNode = new Messages.AddRoadNode
            {
                TemporaryId = Fixture.Create<RoadNodeId>(),
                Type = RoadNodeType.FakeNode,
                Geometry = GeometryTranslator.Translate(geometry2)
            };
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
                {
                    Changes = new[]
                    {
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 1,
                                TemporaryId = Fixture.Create<RoadNodeId>(),
                                Type = RoadNodeType.RealNode,
                                Geometry = GeometryTranslator.Translate(geometry1)
                            }
                        }
                    }
                })
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = addRoadNode
                    }
                ))
                .Then(RoadNetworks.Stream, new RoadNetworkChangesRejected
                {
                    Changes = new[]
                    {
                        new RejectedChange
                        {
                            AddRoadNode = addRoadNode,
                            Reasons = new []
                            {
                                new Reason
                                {
                                    Because = "RoadNodeTooClose",
                                    Parameters = new[]
                                    {
                                        new ReasonParameter
                                        {
                                            Name = "ToOtherNode",
                                            Value = "1"
                                        }
                                    }
                                }
                            }
                        }
                    }
                })
            );
        }
    }
}
