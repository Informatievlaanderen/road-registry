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
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_that_has_not_been_taken()
        {
            var geometry = GeometryTranslator.Translate(Fixture.Create<PointM>());
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 1,
                            Type = RoadNodeType.FakeNode,
                            Geometry2 = geometry
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
                                Type = RoadNodeType.FakeNode,
                                Geometry2 = geometry
                            }
                        }
                    }
                }));
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_taken_after_an_import()
        {
            var geometry1 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var geometry2 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var addRoadNode = new Messages.AddRoadNode
            {
                Id = 1,
                Type = RoadNodeType.FakeNode,
                Geometry2 = geometry1
            };
            return Run(scenario => scenario
                .Given(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = 1,
                    Type = RoadNodeType.RealNode,
                    Geometry2 = geometry2
                })
                .When(TheOperator.ChangesTheRoadNetwork(new RequestedChange
                    {
                        AddRoadNode = addRoadNode
                    }))
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
                                    Because = "RoadNodeIdTaken",
                                    Parameters = new ReasonParameter[0]
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_node_with_an_id_taken_after_a_change()
        {
            var geometry1 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var geometry2 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var addRoadNode = new Messages.AddRoadNode
            {
                Id = 1,
                Type = RoadNodeType.FakeNode,
                Geometry2 = geometry1
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
                                Type = RoadNodeType.RealNode,
                                Geometry2 = geometry2
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
                                    Because = "RoadNodeIdTaken",
                                    Parameters = new ReasonParameter[0]
                                }
                            }
                        }
                    }
                })
            );
        }

        [Fact]
        public Task when_adding_a_node_with_a_geometry_that_has_been_taken()
        {
            var geometry = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var addRoadNode = new Messages.AddRoadNode
            {
                Id = 2,
                Type = RoadNodeType.FakeNode,
                Geometry2 = geometry
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
                                Type = RoadNodeType.RealNode,
                                Geometry2 = geometry
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
            var geometry1 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            var geometry2 = GeometryTranslator.Translate(Fixture.Create<PointM>());
            return Run(scenario => scenario
                .GivenNone()
                .When(TheOperator.ChangesTheRoadNetwork(
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 1,
                            Type = RoadNodeType.FakeNode,
                            Geometry2 = geometry1
                        }
                    },
                    new RequestedChange
                    {
                        AddRoadNode = new Messages.AddRoadNode
                        {
                            Id = 2,
                            Type = RoadNodeType.FakeNode,
                            Geometry2 = geometry2
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
                                Type = RoadNodeType.FakeNode,
                                Geometry2 = geometry1
                            }
                        },
                        new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = 2,
                                Type = RoadNodeType.FakeNode,
                                Geometry2 = geometry2
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
                Id = 2,
                Type = RoadNodeType.FakeNode,
                Geometry2 = GeometryTranslator.Translate(geometry2)
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
                                Type = RoadNodeType.RealNode,
                                Geometry2 = GeometryTranslator.Translate(geometry1)
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
